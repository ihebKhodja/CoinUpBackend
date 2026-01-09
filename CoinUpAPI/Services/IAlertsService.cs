using CoinUpAPI.Dto;

namespace CoinUpAPI.Services
{
    public interface IAlertsService
    {
        Task<List<PriceAlertDto>> GetMyAlertsAsync(string userId);
        Task<PriceAlertDto> CreateAsync(string userId, CreatePriceAlertDto dto);
        Task<PriceAlertDto> UpdateAsync(string userId, string alertId, UpdatePriceAlertDto dto);
        Task DeleteAsync(string userId, string alertId);
    }
}
