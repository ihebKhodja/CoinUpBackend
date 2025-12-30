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

            try
            {
                using var scope = _serviceProvider.CreateScope();

                var collector = scope.ServiceProvider.GetRequiredService<IDataCollectorService>();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();


                // -----------------------------
                // 1. Fetch external data
                // -----------------------------
                var coinsMarkets = await dbContext.CoinsMarket.OrderBy(x => x.Rank).ToListAsync();



                // -----------------------------
                // 4. Fetch Market Chart For Each Coin
                // -----------------------------
                _logger.LogInformation("Fetching market chart ({Days}d) for {Count} coins...", days, coinsMarkets.Count);
                var rateLimitMs = _options.RateLimitMs;

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

                    bool success = false;
                    while (!success)
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
                            success = true;
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

                    // Delay before next coin to respect rate limits
                    if (rateLimitMs > 0)
                    {
                        await Task.Delay(rateLimitMs);
                    }
                }



                // If you have a MarketCharts table
                _logger.LogInformation("🟢 Job terminé avec succès !");
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

        public async Task ExecuteGetHistoryAllAsync()
        {
            var daysOptions = new[] { 1, 7, 30, 90, 365 };
            foreach (var days in daysOptions)
            {
                await ExecuteGetHistoryAsync(days);
            }
        }

    }
}
