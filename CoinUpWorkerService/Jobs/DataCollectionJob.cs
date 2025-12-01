using CoinUpWorkerService.Data;
using CoinUpWorkerService.Services;
using Microsoft.EntityFrameworkCore;
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
        public DataCollectionJob(IServiceProvider serviceProvider, ILogger<DataCollectionJob> logger)

        {
            _serviceProvider = serviceProvider;
            _logger = logger;
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
                dbContext.CoinsMarket.RemoveRange(dbContext.CoinsMarket);
                dbContext.CoinsMarketCategory.RemoveRange(dbContext.CoinsMarketCategory);

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
            }
        }

        public async Task ExecuteGetHistoryAsync()
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
                var coinsMarkets = await dbContext.CoinsMarket.OrderBy(x => x.Rank).ToListAsync();

                dbContext.MarketChartDetails.RemoveRange(dbContext.MarketChartDetails);

                await dbContext.SaveChangesAsync(); // Important to clear table before insert



                // -----------------------------
                // 4. Fetch Market Chart For Each Coin
                // -----------------------------
                _logger.LogInformation("Fetching market chart for {Count} coins...", coinsMarkets.Count);
                var rateLimitMs = 10000;

                foreach (var coin in coinsMarkets)
                {
                    bool success = false;

                    while (!success)
                    {
                        try
                        {
                            var chart = await collector.FetchMarketChartAsync(coin.Id, coin.Rank);

                            if (chart == null)
                            {
                                _logger.LogWarning("Market chart is null for coin {Id}", coin.Id);
                                break; // move to next coin
                            }

                            await dbContext.MarketChartDetails.AddRangeAsync(chart);
                            await dbContext.SaveChangesAsync();

                            _logger.LogInformation("Chart fetched for {Id}", coin.Id);
                            success = true; // exit the retry loop
                        }
                        catch (HttpRequestException ex) when ((int?)ex.StatusCode == 429)
                        {
                            _logger.LogWarning("⚠️ Rate limit hit for {Id}. Waiting {Ms}ms then retrying...",
                                               coin.Id, rateLimitMs);

                            await Task.Delay(rateLimitMs);
                            // loop continues → retry same coin
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "❌ Error fetching market chart for {Id}", coin.Id);
                            break; // don't retry non-429 errors
                        }
                    }

                    // Delay before moving to next coin
                    await Task.Delay(rateLimitMs);
                }



                // If you have a MarketCharts table
                _logger.LogInformation("🟢 Job terminé avec succès !");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "🔴 Erreur dans le job de collecte");
            }
        }

    }
}
