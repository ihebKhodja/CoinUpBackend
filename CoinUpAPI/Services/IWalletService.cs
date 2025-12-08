using CoinUpAPI.Dto;
using CoinUpAPI.Models;

namespace CoinUpAPI.Services
{
    public interface IWalletService
    {
        Task<EWalletDto> GetWalletAsync(string userId);
        Task<BuySellResponseDto> BuyAsync(string userId, BuySellRequestDto dto);
        Task<BuySellResponseDto> SellAsync(string userId, BuySellRequestDto dto);
        Task<IEnumerable<WalletTransactionDto>> GetTransactionsAsync(string userId);
        Task<bool> DepositAsync(string userId, decimal amount);

    }
}
