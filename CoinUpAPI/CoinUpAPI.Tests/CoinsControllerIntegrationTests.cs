using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CoinUp.Shared.Models;
using CoinUpAPI.Data;
using CoinUpAPI.Dto;
using CoinUpAPI.Tests.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoinUpAPI.Tests;

public class CoinsControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public CoinsControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAllCoins_Returns200_WithAuth_AndPagedItems()
    {
        // Arrange: seed DB in the test host
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.CoinsMarket.Add(new CoinsMarket
            {
                Id = "btc",
                Name = "Bitcoin",
                Symbol = "BTC",
                Rank = 1,
                Current_Price = 100m,
                Last_Updated = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        // Act
        var res = await client.GetAsync("/api/coins?page=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var payload = await res.Content.ReadFromJsonAsync<PaginatedCoinsResponse>();
        Assert.NotNull(payload);
        Assert.True(payload!.TotalItems >= 1);
        Assert.Contains(payload.Items, c => c.Id == "btc");
    }

    [Fact]
    public async Task GetMarketChart_Returns404_When_ChartMissing()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.CoinsMarket.Add(new CoinsMarket
            {
                Id = "eth",
                Name = "Ethereum",
                Symbol = "ETH",
                Rank = 1,
                Current_Price = 100m,
                Last_Updated = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        var res = await client.GetAsync("/api/coins/eth/market-chart");
        Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
    }

    [Fact]
    public async Task GetMarketChart_Returns200_When_ChartExists_For90Days()
    {
        var coinId = $"eth-{Guid.NewGuid():N}";

        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            db.CoinsMarket.Add(new CoinsMarket
            {
                Id = coinId,
                Name = "Ethereum",
                Symbol = "ETH",
                Rank = 1,
                Current_Price = 100m,
                Last_Updated = DateTime.UtcNow
            });

            var details = new MarketChartDetails { Id = coinId, Rank = 1 };
            details.Charts = new Dictionary<int, MarketChartWindow>
            {
                [90] = new MarketChartWindow
                {
                    Prices = new() { new() { 1m, 123.45m } },
                    MarketCaps = new() { new() { 1m, 1000m } },
                    TotalVolumes = new() { new() { 1m, 10m } }
                }
            };

            db.MarketChartDetails.Add(details);
            await db.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        var res = await client.GetAsync($"/api/coins/{coinId}/market-chart");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var payload = await res.Content.ReadFromJsonAsync<MarketChartDetailsDto>();
        Assert.NotNull(payload);
        Assert.Equal(coinId, payload!.Id);
        Assert.Equal(90, payload.Days);
        Assert.NotEmpty(payload.Prices);
    }
}
