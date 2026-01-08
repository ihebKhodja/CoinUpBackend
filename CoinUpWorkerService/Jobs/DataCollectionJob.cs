using CoinUpWorkerService.Data;
using CoinUpWorkerService.Services;
using CoinUp.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinUpWorkerService.Jobs
{
    public class DataCollectionJob : IJob
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DataCollectionJob> _logger;
        private readonly DataCollectionJobOptions _options;

        public DataCollectionJob(
            IServiceProvider serviceProvider,
            ILogger<DataCollectionJob> logger,
            IOptions<DataCollectionJobOptions> options)

        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _options = options.Value;
        }

        public async Task ExecuteGetMarketAsync()
        {
            _logger.LogInformation("Job de collecte démarré à {Time}", DateTimeOffset.Now);

            try
            {
                using var scope = _serviceProvider.CreateScope();

                var collector = scope.ServiceProvider.GetRequiredService<IDataCollectorService>();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();


                // -----------------------------
                // 1. Fetch external data
                // -----------------------------
                var coinsMarkets = await collector.FetchCoinsMarketAsync();
                var marketCategories = await collector.FetchMarketCategoriesAsync();


                // -----------------------------
                // 2. DELETE all existing DB rows 
                // -----------------------------
                var existingMarkets = await dbContext.CoinsMarket.ToListAsync();
                var existingCategories = await dbContext.CoinsMarketCategory.ToListAsync();

                dbContext.CoinsMarket.RemoveRange(existingMarkets);
                dbContext.CoinsMarketCategory.RemoveRange(existingCategories);

                await dbContext.SaveChangesAsync(); // Important to clear table before insert


                // -----------------------------
                // 3. INSERT new rows
                // -----------------------------
                await dbContext.CoinsMarket.AddRangeAsync(coinsMarkets);
                await dbContext.CoinsMarketCategory.AddRangeAsync(marketCategories);

                await dbContext.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔴 Erreur dans le job de collecte");
                if (_options.ThrowOnError)
                {
                    throw;
                }
            }
        }

        public async Task ExecuteGetHistoryAsync(int days)
        {
            _logger.LogInformation("Job de collecte (history {Days}d) démarré à {Time}", days, DateTimeOffset.Now);

            var ok = await ExecuteGetHistoryInternalAsync(days);
            if (!ok)
            {
                _logger.LogWarning("❌ History job did not fully succeed for {Days}d", days);
                if (_options.ThrowOnError)
                {
                    throw new InvalidOperationException($"History job failed for {days}d");
                }
            }
        }

        private async Task<bool> ExecuteGetHistoryInternalAsync(int days)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();

                var collector = scope.ServiceProvider.GetRequiredService<IDataCollectorService>();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var coinsMarkets = await dbContext.CoinsMarket.OrderBy(x => x.Rank).ToListAsync();

                _logger.LogInformation("Fetching market chart ({Days}d) for {Count} coins...", days, coinsMarkets.Count);
                var rateLimitMs = _options.RateLimitMs;

                var dayFailed = false;

                foreach (var coin in coinsMarkets)
                {
                    var entity = await dbContext.MarketChartDetails.FindAsync(coin.Id);
                    if (entity == null)
                    {
                        entity = new MarketChartDetails
                        {
                            Id = coin.Id,
                            Rank = coin.Rank,
                            ChartsJson = "{}"
                        };
                        dbContext.MarketChartDetails.Add(entity);
                    }
                    else
                    {
                        entity.Rank = coin.Rank;
                    }

                    var charts = entity.Charts;

                    var coinSuccess = false;
                    while (!coinSuccess)
                    {
                        try
                        {
                            var window = await collector.FetchMarketChartAsync(coin.Id, coin.Rank, days);
                            if (window == null)
                            {
                                _logger.LogWarning("Market chart is null for coin {Id} ({Days}d)", coin.Id, days);
                                break;
                            }

                            charts[days] = window;
                            entity.Charts = charts;

                            await dbContext.SaveChangesAsync();

                            _logger.LogInformation("Chart fetched for {Id} ({Days}d)", coin.Id, days);
                            coinSuccess = true;
                        }
                        catch (HttpRequestException ex) when ((int?)ex.StatusCode == 429)
                        {
                            _logger.LogWarning(
                                "⚠️ Rate limit hit for {Id} ({Days}d). Waiting {Ms}ms then retrying...",
                                coin.Id,
                                days,
                                rateLimitMs
                            );
                            if (rateLimitMs > 0)
                            {
                                await Task.Delay(rateLimitMs);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "❌ Error fetching market chart for {Id} ({Days}d)", coin.Id, days);
                            break;
                        }
                    }

                    if (!coinSuccess)
                    {
                        dayFailed = true;
                    }

                    if (rateLimitMs > 0)
                    {
                        await Task.Delay(rateLimitMs);
                    }
                }

                return !dayFailed;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔴 Erreur dans le job de collecte");
                return false;
            }
        }

        public async Task ExecuteGetHistoryAllAsync()
        {
            var daysOptions = (_options.HistoryDaysOptions?.Length > 0
                    ? _options.HistoryDaysOptions
                    : new[] { 90 })
                .Where(d => d > 0)
                .Distinct()
                .OrderBy(d => d)
                .ToArray();

            if (daysOptions.Length == 0)
            {
                _logger.LogWarning("No HistoryDaysOptions configured; skipping history collection.");
                return;
            }

            foreach (var days in daysOptions)
            {
                var attempt = 0;
                var startedAt = DateTimeOffset.UtcNow;
                var maxRetryMinutes = _options.HistoryWindowMaxRetryMinutes <= 0 ? 30 : _options.HistoryWindowMaxRetryMinutes;
                var maxAttempts = _options.HistoryWindowMaxRetryAttempts < 0 ? 0 : _options.HistoryWindowMaxRetryAttempts;
                while (true)
                {
                    attempt++;
                    var ok = await ExecuteGetHistoryInternalAsync(days);
                    if (ok)
                    {
                        break;
                    }

                    var elapsed = DateTimeOffset.UtcNow - startedAt;
                    if (elapsed >= TimeSpan.FromMinutes(maxRetryMinutes))
                    {
                        _logger.LogError(
                            "❌ History window {Days}d still failing after {ElapsedMinutes:N1} minutes ({Attempt} attempts). Stopping history job.",
                            days,
                            elapsed.TotalMinutes,
                            attempt);

                        if (_options.ThrowOnError)
                        {
                            throw new InvalidOperationException(
                                $"History window {days}d failed for more than {maxRetryMinutes} minutes");
                        }

                        return;
                    }

                    if (maxAttempts > 0 && attempt >= maxAttempts)
                    {
                        _logger.LogError(
                            "❌ History window {Days}d still failing after {Attempt} attempts. Stopping history job.",
                            days,
                            attempt);

                        if (_options.ThrowOnError)
                        {
                            throw new InvalidOperationException($"History window {days}d failed after {attempt} attempts");
                        }

                        return;
                    }

                    _logger.LogWarning(
                        "❌ History window {Days}d failed (attempt {Attempt}). Retrying until success...",
                        days,
                        attempt);

                    var retryDelayMs = Math.Max(0, Math.Max(_options.HistoryWindowRetryDelayMs, _options.RateLimitMs));
                    await Task.Delay(retryDelayMs);
                }
            }
        }

    }
}
