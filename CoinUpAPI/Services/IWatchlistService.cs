using CoinUp.Shared.Models;
using CoinUpAPI.Dto;

namespace CoinUpAPI.Services
{
    public interface IWatchlistService
    {
        Task<WatchlistItemDto?> AddAsync(string userId, string coinId);
        Task<WatchlistItemDto?> RemoveAsync(string userId, string coinId);
        Task<List<WatchlistItemDto>> GetAsync(string userId);
    }

}
