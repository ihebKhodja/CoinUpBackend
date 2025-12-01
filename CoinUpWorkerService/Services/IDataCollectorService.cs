using CoinUp.Shared.Models;

namespace CoinUpWorkerService.Services
{
    public interface IDataCollectorService
    {
        Task<List<CoinsMarket>> FetchCoinsMarketAsync();
        Task<List<CoinsMarketCategory>> FetchMarketCategoriesAsync();
        Task<MarketChartDetails> FetchMarketChartAsync(string id, int rank);


    }


}