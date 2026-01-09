using CoinUpWorkerService.Services;
using CoinUpWorkerService.Tests.TestHelpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using System.Net;

namespace CoinUpWorkerService.Tests;

public class CoinCapServiceTests
{
    [Fact]
    public async Task FetchMarketChartAsync_ParsesWindow()
    {
        var handler = new FakeHttpMessageHandler(_ =>
            FakeHttpMessageHandler.Json(
                HttpStatusCode.OK,
                """{"prices":[[1,10.5],[2,20.25]],"market_caps":[[1,100.0]],"total_volumes":[[1,7.0]]}"""
            )
        );

        var httpClient = new HttpClient(handler);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CoinGecko:BaseUrl"] = "https://api.coingecko.com/api/v3/"
            })
            .Build();

        var service = new CoinCapService(httpClient, configuration, NullLogger<CoinCapService>.Instance);

        var window = await service.FetchMarketChartAsync("bitcoin", rank: 1, days: 1);

        Assert.NotNull(window);
        Assert.Equal(2, window!.Prices.Count);
        Assert.Equal(10.5m, window.Prices[0][1]);
        Assert.Single(window.MarketCaps);
        Assert.Single(window.TotalVolumes);
    }

    [Fact]
    public async Task FetchMarketChartAsync_UsesProHeader_WhenBaseUrlIsPro()
    {
        const string expectedHeader = "x-cg-pro-api-key";

        var handler = new FakeHttpMessageHandler(req =>
        {
            Assert.True(req.Headers.Contains(expectedHeader));
            return FakeHttpMessageHandler.Json(
                HttpStatusCode.OK,
                """{"prices":[[1,1]],"market_caps":[[1,1]],"total_volumes":[[1,1]]}"""
            );
        });

        var httpClient = new HttpClient(handler);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CoinGecko:BaseUrl"] = "https://pro-api.coingecko.com/api/v3/",
                ["CoinGecko:ApiKey"] = "test-key"
            })
            .Build();

        var service = new CoinCapService(httpClient, configuration, NullLogger<CoinCapService>.Instance);

        var window = await service.FetchMarketChartAsync("bitcoin", rank: 1, days: 1);
        Assert.NotNull(window);
    }

    [Fact]
    public async Task FetchCoinsMarketAsync_ReturnsCoinsMarket()
    {
        var handler = new FakeHttpMessageHandler(_ =>
            FakeHttpMessageHandler.Json(
                HttpStatusCode.OK,
                """[{"id":"bitcoin","symbol":"btc","name":"Bitcoin","image":"https://example.com/image.png","current_price":50000,"market_cap":980000000000,"market_cap_rank":1,"fully_diluted_valuation":980000000000,"total_volume":25000000000,"high_24h":51000,"low_24h":49000,"price_change_24h":1000,"price_change_percentage_24h":2.04,"market_cap_change_24h":20000000000,"market_cap_change_percentage_24h":2.08,"circulating_supply":21000000,"total_supply":21000000,"max_supply":21000000,"ath":60000,"ath_change_percentage":-16.67,"ath_date":"2024-01-01T00:00:00Z","atl":100,"atl_change_percentage":49900,"atl_date":"2015-01-01T00:00:00Z","roi":null,"last_updated":"2025-01-08T00:00:00Z"}]"""
            )
        );

        var httpClient = new HttpClient(handler);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CoinGecko:BaseUrl"] = "https://api.coingecko.com/api/v3/"
            })
            .Build();

        var service = new CoinCapService(httpClient, configuration, NullLogger<CoinCapService>.Instance);

        var coins = await service.FetchCoinsMarketAsync();

        Assert.NotNull(coins);
        Assert.Single(coins);
        Assert.Equal("bitcoin", coins[0].Id);
        Assert.Equal("btc", coins[0].Symbol);
        Assert.Equal("Bitcoin", coins[0].Name);
        Assert.Equal(50000m, coins[0].Current_Price);
        Assert.Equal(1, coins[0].Rank);
    }

    [Fact]
    public async Task FetchMarketCategoriesAsync_ReturnsCategoriesList()
    {
        var handler = new FakeHttpMessageHandler(_ =>
            FakeHttpMessageHandler.Json(
                HttpStatusCode.OK,
                """[{"id":"layer-1","name":"Layer 1","market_cap":5000000000,"market_cap_change_24h":-5.2,"content":"Blockchain layer 1 networks"},{"id":"layer-2","name":"Layer 2","market_cap":100000000,"market_cap_change_24h":3.1,"content":"Blockchain layer 2 solutions"}]"""
            )
        );

        var httpClient = new HttpClient(handler);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CoinGecko:BaseUrl"] = "https://api.coingecko.com/api/v3/"
            })
            .Build();

        var service = new CoinCapService(httpClient, configuration, NullLogger<CoinCapService>.Instance);

        var categories = await service.FetchMarketCategoriesAsync();

        Assert.NotNull(categories);
        Assert.Equal(2, categories.Count);
        Assert.Equal("layer-1", categories[0].Id);
        Assert.Equal("Layer 1", categories[0].Name);
    }

    [Fact]
    public async Task FetchMarketChartAsync_ReturnsNullOnHttpError()
    {
        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent("Bad Request", System.Text.Encoding.UTF8, "application/json")
            }
        );

        var httpClient = new HttpClient(handler);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CoinGecko:BaseUrl"] = "https://api.coingecko.com/api/v3/"
            })
            .Build();

        var service = new CoinCapService(httpClient, configuration, NullLogger<CoinCapService>.Instance);

        // FetchMarketChartAsync catches exceptions and returns null
        var result = await service.FetchMarketChartAsync("bitcoin", rank: 1, days: 1);
        Assert.Null(result);
    }

    [Fact]
    public async Task FetchMarketChartAsync_ReturnsNullOnNullJson()
    {
        var handler = new FakeHttpMessageHandler(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
            }
        );

        var httpClient = new HttpClient(handler);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CoinGecko:BaseUrl"] = "https://api.coingecko.com/api/v3/"
            })
            .Build();

        var service = new CoinCapService(httpClient, configuration, NullLogger<CoinCapService>.Instance);

        // FetchMarketChartAsync catches exceptions and returns null
        var result = await service.FetchMarketChartAsync("bitcoin", rank: 1, days: 1);
        Assert.Null(result);
    }
}