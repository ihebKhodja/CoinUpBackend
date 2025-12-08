using CoinUp.Shared.Models;
using CoinUpAPI.Data;
using CoinUpAPI.Dto;
using CoinUpAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CoinUpAPI.Services
{
    public class WatchlistService : IWatchlistService
    {
        private readonly ApplicationDbContext _context;

        public WatchlistService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<WatchlistItemDto> AddAsync(string userId, string coinId)
        {
            // Check if already exists
            var existingItem = await _context.WatchlistItems
                .Include(x => x.Coin)
                .FirstOrDefaultAsync(x => x.UserId == userId && x.CoinId == coinId);

            if (existingItem != null)
            {
                // Already exists → return the existing item
                return new WatchlistItemDto
                {
                    Id = existingItem.Id,
                    CoinId = existingItem.CoinId,
                    CoinName = existingItem.Coin.Name,
                    Symbol = existingItem.Coin.Symbol,
                    Image = existingItem.Coin.Image,
                    CurrentPrice = existingItem.Coin.Current_Price,
                    AddedAt = existingItem.AddedAt
                };
            }

            // Create new watchlist entry
            var watchlistItem = new WatchlistItem
            {
                UserId = userId,
                CoinId = coinId
            };

            _context.WatchlistItems.Add(watchlistItem);
            await _context.SaveChangesAsync();

            // Reload with coin data
            var itemWithCoin = await _context.WatchlistItems
                .Include(x => x.Coin)
                .FirstAsync(x => x.Id == watchlistItem.Id);

            // Return DTO
            return new WatchlistItemDto
            {
                Id = itemWithCoin.Id,
                CoinId = itemWithCoin.CoinId,
                CoinName = itemWithCoin.Coin.Name,
                Symbol = itemWithCoin.Coin.Symbol,
                Image = itemWithCoin.Coin.Image,
                CurrentPrice = itemWithCoin.Coin.Current_Price,
                AddedAt = itemWithCoin.AddedAt
            };
        }


        public async Task<WatchlistItemDto?> RemoveAsync(string userId, string coinId)
        {
            var item = await _context.WatchlistItems
                .Include(x => x.Coin)
                .FirstOrDefaultAsync(x => x.UserId == userId && x.CoinId == coinId);

            if (item == null)
                return null;

            _context.WatchlistItems.Remove(item);
            await _context.SaveChangesAsync();

            return new WatchlistItemDto
            {
                Id = item.Id,
                CoinId = item.CoinId,
                CoinName = item.Coin.Name,
                Symbol = item.Coin.Symbol,
                Image = item.Coin.Image,
                CurrentPrice = item.Coin.Current_Price,
                AddedAt = item.AddedAt
            };
        }


        public async Task<List<WatchlistItemDto>> GetAsync(string userId)
        {
            return await _context.WatchlistItems
                .AsNoTracking()
                .Where(x => x.UserId == userId)
                .Include(x => x.Coin)
                .Select(x => new WatchlistItemDto
                {
                    Id = x.Id,
                    CoinId = x.CoinId,
                    CoinName = x.Coin.Name,
                    Symbol = x.Coin.Symbol,
                    Image = x.Coin.Image,
                    CurrentPrice = x.Coin.Current_Price,
                    AddedAt = x.AddedAt
                })
                .ToListAsync();
        }

    }

}
