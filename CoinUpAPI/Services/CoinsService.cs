using CoinUpAPI.Data;
using CoinUpAPI.Dto;
using Microsoft.EntityFrameworkCore;

namespace CoinUpAPI.Services
{
    public class CoinsService : ICoinsService
    {
        private readonly ApplicationDbContext _dbContext;

        public CoinsService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PaginatedCoinsResponse> GetAllAsync(string? query, int page, int pageSize)
        {
            var coinsQuery = _dbContext.CoinsMarket.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var q = query.Trim().ToLower();
                coinsQuery = coinsQuery.Where(c =>
                    c.Name.ToLower().Contains(q) ||
                    c.Symbol.ToLower().Contains(q)
                );
            }

            var totalItems = await coinsQuery.CountAsync();

            var coins = await coinsQuery
                .OrderBy(c => c.Rank)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new CoinsMarketDto
                {
                    Id = c.Id,
                    Symbol = c.Symbol,
                    Rank = c.Rank,
                    Name = c.Name,
                    Image = c.Image,
                    CurrentPrice = c.Current_Price,
                    MarketCap = c.Market_Cap,
                    MarketCapRank = c.Market_Cap_Rank,
                    FullyDilutedValuation = c.Fully_Diluted_Valuation,
                    TotalVolume = c.Total_Volume,
                    High24h = c.High_24h,
                    Low24h = c.Low_24h,
                    PriceChange24h = c.Price_Change_24h,
                    PriceChangePercentage24h = c.Price_Change_Percentage_24h,
                    MarketCapChange24h = c.Market_Cap_Change_24h,
                    MarketCapChangePercentage24h = c.Market_Cap_Change_Percentage_24h,
                    CirculatingSupply = c.Circulating_Supply,
                    TotalSupply = c.Total_Supply,
                    MaxSupply = c.Max_Supply,
                    Ath = c.Ath,
                    AthChangePercentage = c.Ath_Change_Percentage,
                    AthDate = c.Ath_Date,
                    Atl = c.Atl,
                    AtlChangePercentage = c.Atl_Change_Percentage,
                    AtlDate = c.Atl_Date
                })
                .ToListAsync();

            return new PaginatedCoinsResponse
            {
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                Items = coins
            };
        }

        public async Task<CoinsMarketDto?> GetByIdAsync(string id)
        {
            var coin = await _dbContext.CoinsMarket.FirstOrDefaultAsync(c => c.Id == id);

            if (coin == null)
                return null;

            return new CoinsMarketDto
            {
                Id = coin.Id,
                Symbol = coin.Symbol,
                Rank = coin.Rank,
                Name = coin.Name,
                Image = coin.Image,
                CurrentPrice = coin.Current_Price,
                MarketCap = coin.Market_Cap,
                MarketCapRank = coin.Market_Cap_Rank,
                FullyDilutedValuation = coin.Fully_Diluted_Valuation,
                TotalVolume = coin.Total_Volume,
                High24h = coin.High_24h,
                Low24h = coin.Low_24h,
                PriceChange24h = coin.Price_Change_24h,
                PriceChangePercentage24h = coin.Price_Change_Percentage_24h,
                MarketCapChange24h = coin.Market_Cap_Change_24h,
                MarketCapChangePercentage24h = coin.Market_Cap_Change_Percentage_24h,
                CirculatingSupply = coin.Circulating_Supply,
                TotalSupply = coin.Total_Supply,
                MaxSupply = coin.Max_Supply,
                Ath = coin.Ath,
                AthChangePercentage = coin.Ath_Change_Percentage,
                AthDate = coin.Ath_Date,
                Atl = coin.Atl,
                AtlChangePercentage = coin.Atl_Change_Percentage,
                AtlDate = coin.Atl_Date
            };
        }
        public async Task<decimal> GetCurrentPriceAsync(string coinId)
        {
            var coin = await _dbContext.CoinsMarket.FirstOrDefaultAsync(c => c.Id == coinId);
            if (coin == null) throw new Exception("Coin not found");

            return coin.Current_Price;
        }
    }

}
