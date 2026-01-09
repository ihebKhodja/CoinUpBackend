using CoinUp.Shared.Models;
using CoinUpAPI.Data;
using CoinUpAPI.Services;
using CoinUpAPI.Tests.TestHelpers;
using Xunit;

namespace CoinUpAPI.Tests;

public class CoinsServiceTests
{
    [Fact]
    public async Task GetMarketChartAsync_ReturnsNull_When_CoinMissing()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = new ApplicationDbContext(TestDb.CreateOptions(dbName));

        var service = new CoinsService(db);
        var result = await service.GetMarketChartAsync("btc", days: 7);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetMarketChartAsync_ReturnsNull_When_DaysWindowMissing()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = new ApplicationDbContext(TestDb.CreateOptions(dbName));

        db.MarketChartDetails.Add(new MarketChartDetails
        {
            Id = "btc",
            Rank = 1,
            Charts = new Dictionary<int, MarketChartWindow>
            {
                [1] = new MarketChartWindow
                {
                    Prices = new() { new() { 1m, 100m } },
                    MarketCaps = new() { new() { 1m, 999m } },
                    TotalVolumes = new() { new() { 1m, 50m } },
                }
            }
        });
        await db.SaveChangesAsync();

        var service = new CoinsService(db);
        var result = await service.GetMarketChartAsync("btc", days: 7);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetMarketChartAsync_ReturnsWindow_When_Present()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = new ApplicationDbContext(TestDb.CreateOptions(dbName));

        db.MarketChartDetails.Add(new MarketChartDetails
        {
            Id = "btc",
            Rank = 1,
            Charts = new Dictionary<int, MarketChartWindow>
            {
                [7] = new MarketChartWindow
                {
                    Prices = new() { new() { 1m, 100m }, new() { 2m, 101m } },
                    MarketCaps = new() { new() { 1m, 999m } },
                    TotalVolumes = new() { new() { 1m, 50m } },
                }
            }
        });
        await db.SaveChangesAsync();

        var service = new CoinsService(db);
        var result = await service.GetMarketChartAsync("btc", days: 7);

        Assert.NotNull(result);
        Assert.Equal("btc", result!.Id);
        Assert.Equal(7, result.Days);
        Assert.Equal(2, result.Prices.Count);
    }
}
