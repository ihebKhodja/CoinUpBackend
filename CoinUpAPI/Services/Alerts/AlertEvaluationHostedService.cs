using CoinUpAPI.Data;
using CoinUpAPI.Models;
using CoinUpAPI.Services.Email;
using Microsoft.EntityFrameworkCore;

namespace CoinUpAPI.Services.Alerts
{
    public class AlertEvaluationHostedService : BackgroundService
    {
        private sealed record CoinSnapshot(string Id, string Name, string Symbol, decimal CurrentPrice);

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AlertEvaluationHostedService> _logger;
        private readonly IConfiguration _configuration;

        public AlertEvaluationHostedService(IServiceProvider serviceProvider, ILogger<AlertEvaluationHostedService> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var intervalSeconds = _configuration.GetValue<int?>("AlertEvaluation:IntervalSeconds") ?? 60;
            if (intervalSeconds <= 0) intervalSeconds = 60;

            _logger.LogInformation("Alert evaluator started. Interval: {Seconds}s", intervalSeconds);

            var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));
            try
            {
                // Run once immediately
                await EvaluateOnce(stoppingToken);

                while (await timer.WaitForNextTickAsync(stoppingToken))
                {
                    await EvaluateOnce(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                // shutdown
            }
            finally
            {
                timer.Dispose();
            }
        }

        private async Task EvaluateOnce(CancellationToken token)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var email = scope.ServiceProvider.GetRequiredService<IEmailSender>();

            var now = DateTime.UtcNow;

            var alerts = await db.PriceAlerts
                .Where(a => a.IsActive)
                .ToListAsync(token);

            if (alerts.Count == 0)
            {
                return;
            }

            var coinIds = alerts
                .Select(a => a.CoinId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();
            var coins = await db.CoinsMarket
                .Where(c => coinIds.Contains(c.Id))
                .Select(c => new CoinSnapshot(
                    c.Id,
                    c.Name,
                    c.Symbol,
                    c.Current_Price))
                .ToListAsync(token);

            var coinById = coins.ToDictionary(c => c.Id, c => c);

            var userIds = alerts.Select(a => a.UserId).Distinct().ToList();
            var users = await db.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.Email, u.Username })
                .ToListAsync(token);

            var userById = users.ToDictionary(u => u.Id, u => u);

            // Watchlist membership map
            var watchlistPairs = await db.WatchlistItems
                .Where(w => userIds.Contains(w.UserId) && coinIds.Contains(w.CoinId))
                .Select(w => new { w.UserId, w.CoinId })
                .ToListAsync(token);
            var watchlistSet = watchlistPairs
                .Select(x => (x.UserId, x.CoinId))
                .ToHashSet();

            // Holdings map (AverageBuyPrice)
            var holdingRows = await (from w in db.EWallets
                                     join h in db.CoinHoldings on w.Id equals h.WalletId
                                     where userIds.Contains(w.UserId) && coinIds.Contains(h.CoinId)
                                     select new { w.UserId, h.CoinId, h.AverageBuyPrice })
                .ToListAsync(token);
            var holdingBuyPrice = holdingRows
                .GroupBy(x => (x.UserId, x.CoinId))
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.AverageBuyPrice).First().AverageBuyPrice);

            // Wallet balances
            var wallets = await db.EWallets
                .Where(w => userIds.Contains(w.UserId))
                .Select(w => new { w.UserId, w.Balance })
                .ToListAsync(token);
            var balanceByUser = wallets.ToDictionary(w => w.UserId, w => w.Balance);

            foreach (var alert in alerts)
            {
                if (!userById.TryGetValue(alert.UserId, out var user))
                {
                    continue;
                }

                if (alert.LastTriggeredAt.HasValue)
                {
                    var cooldown = TimeSpan.FromMinutes(alert.CooldownMinutes <= 0 ? 60 : alert.CooldownMinutes);
                    if ((now - alert.LastTriggeredAt.Value) < cooldown)
                    {
                        continue;
                    }
                }

                var evaluation = EvaluateAlert(alert, watchlistSet, holdingBuyPrice, balanceByUser, coinById);
                if (!evaluation.Triggered)
                    continue;

                var subject = evaluation.Subject;
                var body = evaluation.Body;

                var notification = new AlertNotification
                {
                    AlertId = alert.Id,
                    UserId = alert.UserId,
                    Channel = "Email",
                    SentAt = now
                };

                try
                {
                    await email.SendAsync(user.Email, subject, body);
                    notification.Success = true;
                    alert.LastTriggeredAt = now;
                }
                catch (Exception ex)
                {
                    notification.Success = false;
                    notification.Error = ex.Message;
                    _logger.LogError(ex, "Failed to send alert email for alert {AlertId}", alert.Id);
                }

                db.AlertNotifications.Add(notification);
            }

            await db.SaveChangesAsync(token);
        }

        private static (bool Triggered, string Subject, string Body) EvaluateAlert(
            PriceAlert alert,
            HashSet<(string UserId, string CoinId)> watchlistSet,
            Dictionary<(string UserId, string CoinId), decimal> holdingBuyPrice,
            Dictionary<string, decimal> balanceByUser,
            Dictionary<string, CoinSnapshot> coinById)
        {
            // Balance alert
            if (alert.Type == AlertType.WalletBalanceBelow)
            {
                if (!alert.BalanceBelow.HasValue || alert.BalanceBelow.Value <= 0)
                    return (false, string.Empty, string.Empty);

                if (!balanceByUser.TryGetValue(alert.UserId, out var balance))
                    return (false, string.Empty, string.Empty);

                if (balance > alert.BalanceBelow.Value)
                    return (false, string.Empty, string.Empty);

                var subject = "[CoinUp] Solde faible";
                var body = $"Votre solde est faible.\n\n" +
                           $"Solde actuel: {balance}\n" +
                           $"Seuil: {alert.BalanceBelow}\n";
                return (true, subject, body);
            }

            // Coin-based alerts
            if (string.IsNullOrWhiteSpace(alert.CoinId))
                return (false, string.Empty, string.Empty);

            if (!coinById.TryGetValue(alert.CoinId, out var coin))
                return (false, string.Empty, string.Empty);

            decimal currentPrice = coin.CurrentPrice;
            string coinName = coin.Name;
            string symbol = coin.Symbol;

            switch (alert.Type)
            {
                case AlertType.WatchlistPrice:
                    // Only for coins in watchlist
                    if (!watchlistSet.Contains((alert.UserId, alert.CoinId)))
                        return (false, string.Empty, string.Empty);

                    var above = alert.AbovePrice.HasValue && alert.AbovePrice.Value > 0 && currentPrice >= alert.AbovePrice.Value;
                    var below = alert.BelowPrice.HasValue && alert.BelowPrice.Value > 0 && currentPrice <= alert.BelowPrice.Value;
                    if (!above && !below)
                        return (false, string.Empty, string.Empty);

                    return (true,
                        $"[CoinUp] Watchlist alert: {coinName} ({symbol})",
                        $"Alerte watchlist déclenchée.\n\n" +
                        $"Coin: {coinName} ({symbol})\n" +
                        $"Prix actuel: {currentPrice}\n" +
                        $"Seuil haut: {alert.AbovePrice}\n" +
                        $"Seuil bas: {alert.BelowPrice}\n");

                case AlertType.WalletPriceVsBuy:
                    // Only for coins in holdings
                    if (!holdingBuyPrice.TryGetValue((alert.UserId, alert.CoinId), out var avgBuy) || avgBuy <= 0)
                        return (false, string.Empty, string.Empty);

                    var triggeredAbove = false;
                    var triggeredBelow = false;

                    if (alert.AbovePercentFromBuy.HasValue && alert.AbovePercentFromBuy.Value > 0)
                    {
                        var target = avgBuy * (1 + (alert.AbovePercentFromBuy.Value / 100m));
                        triggeredAbove = currentPrice >= target;
                    }

                    if (alert.BelowPercentFromBuy.HasValue && alert.BelowPercentFromBuy.Value > 0)
                    {
                        var target = avgBuy * (1 - (alert.BelowPercentFromBuy.Value / 100m));
                        triggeredBelow = currentPrice <= target;
                    }

                    if (!triggeredAbove && !triggeredBelow)
                        return (false, string.Empty, string.Empty);

                    return (true,
                        $"[CoinUp] Wallet alert: {coinName} ({symbol})",
                        $"Alerte wallet déclenchée.\n\n" +
                        $"Coin: {coinName} ({symbol})\n" +
                        $"Prix actuel: {currentPrice}\n" +
                        $"Prix moyen d'achat: {avgBuy}\n" +
                        $"Seuil +%: {alert.AbovePercentFromBuy}\n" +
                        $"Seuil -%: {alert.BelowPercentFromBuy}\n");

                default:
                    return (false, string.Empty, string.Empty);
            }
        }
    }
}
