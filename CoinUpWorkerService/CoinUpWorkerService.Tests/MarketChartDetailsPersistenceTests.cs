using CoinUp.Shared.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CoinUpWorkerService.Tests;

public class MarketChartDetailsPersistenceTests
{
    [Fact]
    public async Task MarketChartDetails_PersistsChartsJson_AndRehydratesCharts()
    {
        using var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ChartsDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var db = new ChartsDbContext(options))
        {
            await db.Database.EnsureCreatedAsync();

            var entity = new MarketChartDetails
            {
                Id = "bitcoin",
                Rank = 1
            };

            entity.Charts = new Dictionary<int, MarketChartWindow>
            {
                [90] = new MarketChartWindow
                {
                    Prices = new() { new() { 1m, 42m } }
                }
            };

            db.MarketChartDetails.Add(entity);
            await db.SaveChangesAsync();
        }

        await using (var db = new ChartsDbContext(options))
        {
            var loaded = await db.MarketChartDetails.SingleAsync(x => x.Id == "bitcoin");
            Assert.True(loaded.Charts.ContainsKey(90));
            Assert.Equal(42m, loaded.Charts[90].Prices[0][1]);
        }
    }

    private sealed class ChartsDbContext : DbContext
    {
        public ChartsDbContext(DbContextOptions<ChartsDbContext> options) : base(options) { }
        public DbSet<MarketChartDetails> MarketChartDetails => Set<MarketChartDetails>();
    }
}
