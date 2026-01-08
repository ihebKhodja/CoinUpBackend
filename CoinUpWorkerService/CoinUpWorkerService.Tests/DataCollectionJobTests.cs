using CoinUp.Shared.Models;
using CoinUpWorkerService.Data;
using CoinUpWorkerService.Jobs;
using CoinUpWorkerService.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace CoinUpWorkerService.Tests;

public class DataCollectionJobTests
{
    [Fact]
    public async Task ExecuteGetMarketAsync_ReplacesExistingRows()
    {
        var databaseRoot = new InMemoryDatabaseRoot();
        var databaseName = $"{nameof(ExecuteGetMarketAsync_ReplacesExistingRows)}-{Guid.NewGuid()}";

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName, databaseRoot));
        services.AddScoped<IDataCollectorService>(_ => new FakeCollectorService());
        services.AddOptions<DataCollectionJobOptions>().Configure(o =>
        {
            o.RateLimitMs = 0;
            o.ThrowOnError = true;
            o.HistoryWindowRetryDelayMs = 0;
        });
        services.AddScoped<DataCollectionJob>();

        await using var provider = services.BuildServiceProvider();

        // Ensure schema
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.EnsureCreatedAsync();

            db.CoinsMarket.Add(new CoinsMarket { Id = "old", Name = "Old", Symbol = "old", Rank = 999 });
            db.CoinsMarketCategory.Add(new CoinsMarketCategory { Id = "oldcat", Name = "OldCat" });
            await db.SaveChangesAsync();
        }

        var job = provider.GetRequiredService<DataCollectionJob>();
        await job.ExecuteGetMarketAsync();

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var markets = await db.CoinsMarket.OrderBy(x => x.Id).ToListAsync();
            var cats = await db.CoinsMarketCategory.OrderBy(x => x.Id).ToListAsync();

            Assert.DoesNotContain(markets, x => x.Id == "old");
            Assert.DoesNotContain(cats, x => x.Id == "oldcat");

            Assert.Contains(markets, x => x.Id == "bitcoin");
            Assert.Contains(cats, x => x.Id == "layer-1");
        }
    }

    [Fact]
    public async Task ExecuteGetHistoryAsync_PersistsWindowInChartsJson()
    {
        var databaseRoot = new InMemoryDatabaseRoot();
        var databaseName = $"{nameof(ExecuteGetHistoryAsync_PersistsWindowInChartsJson)}-{Guid.NewGuid()}";

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName, databaseRoot));
        services.AddScoped<IDataCollectorService>(_ => new FakeCollectorService());
        services.AddOptions<DataCollectionJobOptions>().Configure(o =>
        {
            o.RateLimitMs = 0;
            o.ThrowOnError = true;
            o.HistoryWindowRetryDelayMs = 0;
        });
        services.AddScoped<DataCollectionJob>();

        await using var provider = services.BuildServiceProvider();

        // Ensure schema + seed one coin
        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.EnsureCreatedAsync();

            db.CoinsMarket.Add(new CoinsMarket { Id = "bitcoin", Name = "Bitcoin", Symbol = "btc", Rank = 1 });
            await db.SaveChangesAsync();
        }

        var job = provider.GetRequiredService<DataCollectionJob>();
        await job.ExecuteGetHistoryAsync(days: 7);

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var entity = await db.MarketChartDetails.FindAsync("bitcoin");

            Assert.NotNull(entity);
            Assert.True(entity!.Charts.ContainsKey(7));
            Assert.Single(entity.Charts[7].Prices);
            Assert.Equal(123.45m, entity.Charts[7].Prices[0][1]);
        }
    }

    [Fact]
    public async Task ExecuteGetHistoryAllAsync_StopsAfterMaxAttempts_WhenFailuresPersist()
    {
        var databaseRoot = new InMemoryDatabaseRoot();
        var databaseName = $"{nameof(ExecuteGetHistoryAllAsync_StopsAfterMaxAttempts_WhenFailuresPersist)}-{Guid.NewGuid()}";

        var failingCollector = new AlwaysNullChartCollector();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase(databaseName, databaseRoot));
        services.AddScoped<IDataCollectorService>(_ => failingCollector);
        services.AddOptions<DataCollectionJobOptions>().Configure(o =>
        {
            o.RateLimitMs = 0;
            o.ThrowOnError = false;
            o.HistoryWindowRetryDelayMs = 0;
            o.HistoryWindowMaxRetryMinutes = 30;
            o.HistoryWindowMaxRetryAttempts = 2;
            o.HistoryDaysOptions = new[] { 7 };
        });
        services.AddScoped<DataCollectionJob>();

        await using var provider = services.BuildServiceProvider();

        using (var scope = provider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.EnsureCreatedAsync();

            db.CoinsMarket.Add(new CoinsMarket { Id = "bitcoin", Name = "Bitcoin", Symbol = "btc", Rank = 1 });
            await db.SaveChangesAsync();
        }

        var job = provider.GetRequiredService<DataCollectionJob>();
        await job.ExecuteGetHistoryAllAsync();

        Assert.True(failingCollector.Calls >= 2);
    }

    private sealed class FakeCollectorService : IDataCollectorService
    {
        public Task<List<CoinsMarket>> FetchCoinsMarketAsync()
        {
            return Task.FromResult(new List<CoinsMarket>
            {
                new() { Id = "bitcoin", Name = "Bitcoin", Symbol = "btc", Rank = 1 }
            });
        }

        public Task<List<CoinsMarketCategory>> FetchMarketCategoriesAsync()
        {
            return Task.FromResult(new List<CoinsMarketCategory>
            {
                new() { Id = "layer-1", Name = "Layer 1" }
            });
        }

        public Task<MarketChartWindow?> FetchMarketChartAsync(string id, int rank, int days = 90)
        {
            return Task.FromResult<MarketChartWindow?>(new MarketChartWindow
            {
                Prices = new() { new() { 1m, 123.45m } },
                MarketCaps = new() { new() { 1m, 1000m } },
                TotalVolumes = new() { new() { 1m, 10m } }
            });
        }
    }

    private sealed class AlwaysNullChartCollector : IDataCollectorService
    {
        private int _calls;
        public int Calls => _calls;

        public Task<List<CoinsMarket>> FetchCoinsMarketAsync()
            => Task.FromResult(new List<CoinsMarket>());

        public Task<List<CoinsMarketCategory>> FetchMarketCategoriesAsync()
            => Task.FromResult(new List<CoinsMarketCategory>());

        public Task<MarketChartWindow?> FetchMarketChartAsync(string id, int rank, int days = 90)
        {
            Interlocked.Increment(ref _calls);
            return Task.FromResult<MarketChartWindow?>(null);
        }
    }
}
