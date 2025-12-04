using CoinUp.Shared.Models;

namespace CoinUpAPI.Services
{
    public interface IWatchlistService
    {
        Task AddAsync(string userId, string coinId);
        Task RemoveAsync(string userId, string coinId);
        Task<IEnumerable<CoinsMarket>> GetAsync(string userId);
    }

}
