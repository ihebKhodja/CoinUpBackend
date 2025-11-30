using CoinUpAPI.Dto;

namespace CoinUpAPI.Services
{
    public interface ICoinsService
    {
        Task<PaginatedCoinsResponse> GetAllAsync(string? query, int page, int pageSize);
        Task<CoinsMarketDto?> GetByIdAsync(string id);
    }
}
