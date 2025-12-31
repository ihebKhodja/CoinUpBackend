using CoinUpAPI.Data;
using CoinUpAPI.Dto;
using CoinUpAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CoinUpAPI.Services
{
    public class AlertsService : IAlertsService
    {
        private readonly ApplicationDbContext _db;

        public AlertsService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<PriceAlertDto>> GetMyAlertsAsync(string userId)
        {
            return await _db.PriceAlerts
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .Select(a => new PriceAlertDto
                {
                    Id = a.Id,
                    CoinId = a.CoinId,
                    Type = a.Type,
                    AbovePrice = a.AbovePrice,
                    BelowPrice = a.BelowPrice,
                    AbovePercentFromBuy = a.AbovePercentFromBuy,
                    BelowPercentFromBuy = a.BelowPercentFromBuy,
                    BalanceBelow = a.BalanceBelow,
                    IsActive = a.IsActive,
                    CooldownMinutes = a.CooldownMinutes,
                    CreatedAt = a.CreatedAt,
                    LastTriggeredAt = a.LastTriggeredAt
                })
                .ToListAsync();
        }

        public async Task<PriceAlertDto> CreateAsync(string userId, CreatePriceAlertDto dto)
        {
            await ValidateAndEnforceScopeAsync(userId, dto.Type, dto.CoinId, dto.AbovePrice, dto.BelowPrice, dto.AbovePercentFromBuy, dto.BelowPercentFromBuy, dto.BalanceBelow);

            var alert = new PriceAlert
            {
                UserId = userId,
                CoinId = string.IsNullOrWhiteSpace(dto.CoinId) ? null : dto.CoinId,
                Type = dto.Type,
                AbovePrice = dto.AbovePrice,
                BelowPrice = dto.BelowPrice,
                AbovePercentFromBuy = dto.AbovePercentFromBuy,
                BelowPercentFromBuy = dto.BelowPercentFromBuy,
                BalanceBelow = dto.BalanceBelow,
                CooldownMinutes = dto.CooldownMinutes <= 0 ? 60 : dto.CooldownMinutes,
                IsActive = dto.IsActive
            };

            _db.PriceAlerts.Add(alert);
            await _db.SaveChangesAsync();

            return new PriceAlertDto
            {
                Id = alert.Id,
                CoinId = alert.CoinId,
                Type = alert.Type,
                AbovePrice = alert.AbovePrice,
                BelowPrice = alert.BelowPrice,
                AbovePercentFromBuy = alert.AbovePercentFromBuy,
                BelowPercentFromBuy = alert.BelowPercentFromBuy,
                BalanceBelow = alert.BalanceBelow,
                IsActive = alert.IsActive,
                CooldownMinutes = alert.CooldownMinutes,
                CreatedAt = alert.CreatedAt,
                LastTriggeredAt = alert.LastTriggeredAt
            };
        }

        public async Task<PriceAlertDto> UpdateAsync(string userId, string alertId, UpdatePriceAlertDto dto)
        {
            var alert = await _db.PriceAlerts.FirstOrDefaultAsync(a => a.Id == alertId && a.UserId == userId);
            if (alert == null)
            {
                throw new KeyNotFoundException("Alert not found");
            }

            var newType = dto.Type ?? alert.Type;
            var newCoinId = alert.CoinId;

            var newAbovePrice = dto.AbovePrice ?? alert.AbovePrice;
            var newBelowPrice = dto.BelowPrice ?? alert.BelowPrice;
            var newAbovePct = dto.AbovePercentFromBuy ?? alert.AbovePercentFromBuy;
            var newBelowPct = dto.BelowPercentFromBuy ?? alert.BelowPercentFromBuy;
            var newBalanceBelow = dto.BalanceBelow ?? alert.BalanceBelow;

            await ValidateAndEnforceScopeAsync(userId, newType, newCoinId, newAbovePrice, newBelowPrice, newAbovePct, newBelowPct, newBalanceBelow);

            alert.Type = newType;
            alert.AbovePrice = newAbovePrice;
            alert.BelowPrice = newBelowPrice;
            alert.AbovePercentFromBuy = newAbovePct;
            alert.BelowPercentFromBuy = newBelowPct;
            alert.BalanceBelow = newBalanceBelow;

            if (dto.IsActive.HasValue) alert.IsActive = dto.IsActive.Value;
            if (dto.CooldownMinutes.HasValue) alert.CooldownMinutes = dto.CooldownMinutes.Value <= 0 ? 60 : dto.CooldownMinutes.Value;

            await _db.SaveChangesAsync();

            return new PriceAlertDto
            {
                Id = alert.Id,
                CoinId = alert.CoinId,
                Type = alert.Type,
                AbovePrice = alert.AbovePrice,
                BelowPrice = alert.BelowPrice,
                AbovePercentFromBuy = alert.AbovePercentFromBuy,
                BelowPercentFromBuy = alert.BelowPercentFromBuy,
                BalanceBelow = alert.BalanceBelow,
                IsActive = alert.IsActive,
                CooldownMinutes = alert.CooldownMinutes,
                CreatedAt = alert.CreatedAt,
                LastTriggeredAt = alert.LastTriggeredAt
            };
        }

        public async Task DeleteAsync(string userId, string alertId)
        {
            var alert = await _db.PriceAlerts.FirstOrDefaultAsync(a => a.Id == alertId && a.UserId == userId);
            if (alert == null)
            {
                throw new KeyNotFoundException("Alert not found");
            }

            _db.PriceAlerts.Remove(alert);
            await _db.SaveChangesAsync();
        }

        private async Task ValidateAndEnforceScopeAsync(
            string userId,
            AlertType type,
            string? coinId,
            decimal? abovePrice,
            decimal? belowPrice,
            decimal? abovePercentFromBuy,
            decimal? belowPercentFromBuy,
            decimal? balanceBelow)
        {
            switch (type)
            {
                case AlertType.WatchlistPrice:
                    if (string.IsNullOrWhiteSpace(coinId))
                        throw new ArgumentException("CoinId is required for watchlist alerts");

                    if ((!abovePrice.HasValue || abovePrice.Value <= 0) && (!belowPrice.HasValue || belowPrice.Value <= 0))
                        throw new ArgumentException("Set AbovePrice and/or BelowPrice (> 0) for watchlist alerts");

                    // Must be in user's watchlist
                    var inWatchlist = await _db.WatchlistItems.AnyAsync(w => w.UserId == userId && w.CoinId == coinId);
                    if (!inWatchlist)
                        throw new ArgumentException("Coin must be in your watchlist to create this alert");

                    break;

                case AlertType.WalletPriceVsBuy:
                    if (string.IsNullOrWhiteSpace(coinId))
                        throw new ArgumentException("CoinId is required for wallet alerts");

                    if ((!abovePercentFromBuy.HasValue || abovePercentFromBuy.Value <= 0) && (!belowPercentFromBuy.HasValue || belowPercentFromBuy.Value <= 0))
                        throw new ArgumentException("Set AbovePercentFromBuy and/or BelowPercentFromBuy (> 0) for wallet alerts");

                    // Must be in user's wallet holdings
                    var inHoldings = await (from w in _db.EWallets
                                            join h in _db.CoinHoldings on w.Id equals h.WalletId
                                            where w.UserId == userId && h.CoinId == coinId
                                            select h.Id)
                        .AnyAsync();
                    if (!inHoldings)
                        throw new ArgumentException("Coin must be in your wallet holdings to create this alert");

                    break;

                case AlertType.WalletBalanceBelow:
                    if (!balanceBelow.HasValue || balanceBelow.Value <= 0)
                        throw new ArgumentException("BalanceBelow must be set and > 0 for low-balance alerts");
                    break;

                default:
                    throw new ArgumentException("Unsupported alert type");
            }
        }
    }
}
