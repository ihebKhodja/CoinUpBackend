using CoinUpAPI.Models;

namespace CoinUpAPI.Services
{
    public interface IPortfolioService
    {
        Task CreateSnapshotAsync(string userId);
        Task<IEnumerable<PortfolioSnapshot>> GetSnapshotsAsync(string userId);
    }
}

