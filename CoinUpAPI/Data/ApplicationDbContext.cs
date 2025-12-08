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


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Wallets
            modelBuilder.Entity<EWallet>()
                .Property(w => w.Balance)
                .HasColumnType("decimal(18,8)");

            // Holdings
            modelBuilder.Entity<CoinHolding>()
                .Property(h => h.Quantity)
                .HasColumnType("decimal(18,8)");
            modelBuilder.Entity<CoinHolding>()
                .Property(h => h.AverageBuyPrice)
                .HasColumnType("decimal(18,8)");

            // Transactions
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Quantity)
                .HasColumnType("decimal(18,8)");
            modelBuilder.Entity<Transaction>()
                .Property(t => t.PriceAtOperation)
                .HasColumnType("decimal(18,8)");

            // Market data
            modelBuilder.Entity<CoinsMarket>()
                .Property(c => c.Current_Price)
                .HasColumnType("decimal(18,8)");
            modelBuilder.Entity<CoinsMarket>()
                .Property(c => c.Market_Cap)
                .HasColumnType("decimal(18,2)");
            modelBuilder.Entity<CoinsMarket>()
                .Property(c => c.Total_Volume)
                .HasColumnType("decimal(18,2)");
        }

    }
}
