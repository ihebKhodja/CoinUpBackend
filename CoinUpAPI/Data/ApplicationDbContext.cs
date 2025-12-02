using CoinUp.Shared.Models;
using CoinUpAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CoinUpAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<CoinsMarket> CoinsMarket { get; set; }
        public DbSet<CoinsMarketCategory> CoinsMarketCategory { get; set; }
        public DbSet<MarketChartDetails> MarketChartDetails { get; set; }

        public DbSet<User> Users { get; set; }
        public DbSet<EWallet> EWallets { get; set; }
        public DbSet<CoinHolding> CoinHoldings { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<WatchlistItem> WatchlistItems { get; set; }
        public DbSet<PortfolioSnapshot> PortfolioSnapshots { get; set; }

    }
}
