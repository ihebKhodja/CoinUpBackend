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
}