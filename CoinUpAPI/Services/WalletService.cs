using CoinUpAPI.Data;
using CoinUpAPI.Dto;
using CoinUpAPI.Models;
using CoinUpAPI.Services.Email;
using Microsoft.EntityFrameworkCore;

namespace CoinUpAPI.Services
{
    public class WalletService : IWalletService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICoinsService _market;
        private readonly IEmailSender _email;
        private readonly ILogger<WalletService> _logger;

        public WalletService(ApplicationDbContext context, ICoinsService market, IEmailSender email, ILogger<WalletService> logger)
        {
            _context = context;
            _market = market;
            _email = email;
            _logger = logger;
        }

        public async Task<EWalletDto> GetWalletAsync(string userId)
        {
            // 1️⃣ Try to load existing wallet
            var wallet = await _context.EWallets
                .Include(w => w.Holdings)
                .ThenInclude(h => h.Coin)
                .FirstOrDefaultAsync(w => w.UserId == userId);

            // 2️⃣ If no wallet exists → create one
            if (wallet == null)
            {
                wallet = new EWallet
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Balance = 0,
                    Holdings = new List<CoinHolding>()
                };

                _context.EWallets.Add(wallet);
                await _context.SaveChangesAsync();
            }

            // 3️⃣ Calculate profit & current value for each holding
            foreach (var holding in wallet.Holdings)
            {
                var price = await _market.GetCurrentPriceAsync(holding.CoinId);

                holding.CurrentValue = holding.Quantity * price;
                holding.Profit = holding.CurrentValue - (holding.AverageBuyPrice * holding.Quantity);
            }

            // 4️⃣ Map manually to DTO
            return new EWalletDto
            {
                WalletId = wallet.Id,
                Balance = wallet.Balance,
                Holdings = wallet.Holdings.Select(h => new CoinHoldingDto
                {
                    CoinId = h.CoinId,
                    Symbol = h.Coin?.Symbol,
                    Quantity = h.Quantity,
                    AverageBuyPrice = h.AverageBuyPrice,
                    CurrentValue = h.CurrentValue,
                    Profit = h.Profit,
                    LastUpdated = h.LastUpdated
                }).ToList()
            };
        }



        public async Task<BuySellResponseDto> BuyAsync(string userId, BuySellRequestDto dto)
        {
            // 1️⃣ Get wallet with holdings
            var wallet = await _context.EWallets
                .Include(w => w.Holdings)
                .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
            {
                // Create wallet if missing
                wallet = new EWallet
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Balance = 0,
                    Holdings = new List<CoinHolding>()
                };
                await _context.EWallets.AddAsync(wallet);
                await _context.SaveChangesAsync();
            }

            // 2️⃣ Get current coin price
            var price = await _market.GetCurrentPriceAsync(dto.CoinId);
            var cost = dto.Quantity * price;

            if (wallet.Balance < cost)
                throw new Exception("Insufficient balance");

            wallet.Balance -= cost;

            // 3️⃣ Update or create holding
            var holding = wallet.Holdings.FirstOrDefault(h => h.CoinId == dto.CoinId);
            if (holding == null)
            {
                holding = new CoinHolding
                {
                    Id = Guid.NewGuid().ToString(),
                    WalletId = wallet.Id,
                    CoinId = dto.CoinId,
                    Quantity = dto.Quantity,
                    AverageBuyPrice = price,
                    LastUpdated = DateTime.UtcNow
                };
                wallet.Holdings.Add(holding);
            }
            else
            {
                var totalCostBefore = holding.AverageBuyPrice * holding.Quantity;
                var totalCostAfter = totalCostBefore + cost;
                holding.Quantity += dto.Quantity;
                holding.AverageBuyPrice = totalCostAfter / holding.Quantity;
                holding.LastUpdated = DateTime.UtcNow;
            }

            // 4️⃣ Record transaction
            var transaction = new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                WalletId = wallet.Id,
                CoinId = dto.CoinId,
                Type = TransactionType.Buy,
                Quantity = dto.Quantity,
                PriceAtOperation = price,
                Timestamp = DateTime.UtcNow
            };

            await _context.Transactions.AddAsync(transaction);

            // 5️⃣ Save all changes
            await _context.SaveChangesAsync();

            await TrySendWalletEmailAsync(
                userId,
                subject: "[CoinUp] Achat effectué",
                body: $"Achat confirmé.\n\nCoin: {dto.CoinId}\nQuantité: {dto.Quantity}\nPrix unitaire: {price}\nCoût: {cost}\nNouveau solde: {wallet.Balance}\n");

            await CheckAndTriggerLowBalanceAlertsAsync(userId, wallet.Balance);

            // 6️⃣ Return response
            return new BuySellResponseDto
            {
                CoinId = dto.CoinId,
                Quantity = dto.Quantity,
                Price = price,
                Type = "BUY",
                NewBalance = wallet.Balance
            };
        }


        public async Task<BuySellResponseDto> SellAsync(string userId, BuySellRequestDto dto)
        {
            var wallet = await _context.EWallets
                .Include(w => w.Holdings)
                .FirstOrDefaultAsync(w => w.UserId == userId);

            var holding = wallet.Holdings
                .FirstOrDefault(h => h.CoinId == dto.CoinId);

            if (holding == null || holding.Quantity < dto.Quantity)
                throw new Exception("Not enough coins to sell");

            var price = await _market.GetCurrentPriceAsync(dto.CoinId);
            var revenue = dto.Quantity * price;

            holding.Quantity -= dto.Quantity;
            if (holding.Quantity == 0) _context.CoinHoldings.Remove(holding);

            wallet.Balance += revenue;

            _context.Transactions.Add(new Transaction
            {
                Id = Guid.NewGuid().ToString(),
                WalletId = wallet.Id,
                CoinId = dto.CoinId,
                Type = TransactionType.Sell,
                Quantity = dto.Quantity,
                PriceAtOperation = price,
                Timestamp = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            await TrySendWalletEmailAsync(
                userId,
                subject: "[CoinUp] Vente effectuée",
                body: $"Vente confirmée.\n\nCoin: {dto.CoinId}\nQuantité: {dto.Quantity}\nPrix unitaire: {price}\nRevenu: {revenue}\nNouveau solde: {wallet.Balance}\n");

            await CheckAndTriggerLowBalanceAlertsAsync(userId, wallet.Balance);

            return new BuySellResponseDto
            {
                CoinId = dto.CoinId,
                Quantity = dto.Quantity,
                Price = price,
                Type = "SELL",
                NewBalance = wallet.Balance
            };
        }

        public async Task<IEnumerable<WalletTransactionDto>> GetTransactionsAsync(string userId)
        {
            var wallet = await _context.EWallets
                                       .Include(w => w.Holdings)
                                       .FirstOrDefaultAsync(w => w.UserId == userId);
            if (wallet == null)
                return new List<WalletTransactionDto>();

            var transactions = await _context.Transactions
                .Where(t => t.WalletId == wallet.Id)
                .OrderByDescending(t => t.Timestamp)
                .Select(t => new WalletTransactionDto
                {
                    Id = t.Id,
                    CoinId = t.CoinId,
                    Type = t.Type.ToString(),
                    Quantity = t.Quantity,
                    PriceAtOperation = t.PriceAtOperation,
                    Timestamp = t.Timestamp
                })
                .ToListAsync();

            return transactions;
        }

        public async Task<bool> DepositAsync(string userId, decimal amount)
        {
            if (amount <= 0) throw new ArgumentException("Amount must be positive.");

            var wallet = await _context.EWallets
                           .Include(w => w.Holdings)
                           .FirstOrDefaultAsync(w => w.UserId == userId);

            if (wallet == null)
            {
                wallet = new EWallet
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    Balance = 0,
                    Holdings = new List<CoinHolding>()
                };
                await _context.EWallets.AddAsync(wallet);
            }

            wallet.Balance += amount;

            await _context.SaveChangesAsync();

            await CheckAndTriggerLowBalanceAlertsAsync(userId, wallet.Balance);

            return true;
        }

        private async Task CheckAndTriggerLowBalanceAlertsAsync(string userId, decimal currentBalance)
        {
            try
            {
                var now = DateTime.UtcNow;

                var alerts = await _context.PriceAlerts
                    .Where(a => a.UserId == userId && a.IsActive && a.Type == AlertType.WalletBalanceBelow && a.BalanceBelow.HasValue)
                    .ToListAsync();

                if (alerts.Count == 0)
                    return;

                foreach (var alert in alerts)
                {
                    var threshold = alert.BalanceBelow!.Value;
                    if (currentBalance > threshold)
                        continue;

                    if (alert.LastTriggeredAt.HasValue)
                    {
                        var cooldown = TimeSpan.FromMinutes(alert.CooldownMinutes <= 0 ? 60 : alert.CooldownMinutes);
                        if ((now - alert.LastTriggeredAt.Value) < cooldown)
                            continue;
                    }

                    var user = await _context.Users
                        .AsNoTracking()
                        .Where(u => u.Id == userId)
                        .Select(u => new { u.Email })
                        .FirstOrDefaultAsync();

                    if (user == null || string.IsNullOrWhiteSpace(user.Email))
                        continue;

                    var subject = "[CoinUp] Solde faible";
                    var body = $"Votre solde est passé sous le seuil configuré.\n\n" +
                               $"Solde actuel: {currentBalance}\n" +
                               $"Seuil: {threshold}\n";

                    await _email.SendAsync(user.Email, subject, body);

                    alert.LastTriggeredAt = now;
                    _context.AlertNotifications.Add(new AlertNotification
                    {
                        AlertId = alert.Id,
                        UserId = userId,
                        Channel = "Email",
                        Success = true,
                        SentAt = now
                    });
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to trigger low-balance alerts.");
            }
        }

        private async Task TrySendWalletEmailAsync(string userId, string subject, string body)
        {
            try
            {
                var user = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.Id == userId)
                    .Select(u => new { u.Email })
                    .FirstOrDefaultAsync();

                if (user == null || string.IsNullOrWhiteSpace(user.Email))
                {
                    return;
                }

                await _email.SendAsync(user.Email, subject, body);
            }
            catch (Exception ex)
            {
                // Do not fail wallet operations if email fails
                _logger.LogWarning(ex, "Failed to send wallet notification email.");
            }
        }
    }

}
