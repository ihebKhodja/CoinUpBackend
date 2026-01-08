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

        public DbSet<PriceAlert> PriceAlerts { get; set; }
        public DbSet<AlertNotification> AlertNotifications { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(u => u.Role)
                .HasMaxLength(16)
                .HasDefaultValue("User");

            modelBuilder.Entity<User>()
                .Property(u => u.IsActive)
                .HasDefaultValue(true);

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
            modelBuilder.Entity<CoinsMarket>(entity =>
            {
                // Prices
                entity.Property(c => c.Current_Price).HasColumnType("decimal(38,18)");
                entity.Property(c => c.High_24h).HasColumnType("decimal(38,18)");
                entity.Property(c => c.Low_24h).HasColumnType("decimal(38,18)");
                entity.Property(c => c.Price_Change_24h).HasColumnType("decimal(38,18)");

                // Market caps / volumes
                entity.Property(c => c.Market_Cap).HasColumnType("decimal(38,2)");
                entity.Property(c => c.Fully_Diluted_Valuation).HasColumnType("decimal(38,2)");
                entity.Property(c => c.Total_Volume).HasColumnType("decimal(38,2)");
                entity.Property(c => c.Market_Cap_Change_24h).HasColumnType("decimal(38,2)");

                // Percent changes
                entity.Property(c => c.Price_Change_Percentage_24h).HasColumnType("decimal(18,8)");
                entity.Property(c => c.Market_Cap_Change_Percentage_24h).HasColumnType("decimal(18,8)");
                entity.Property(c => c.Ath_Change_Percentage).HasColumnType("decimal(18,8)");
                entity.Property(c => c.Atl_Change_Percentage).HasColumnType("decimal(18,8)");

                // Supply
                entity.Property(c => c.Circulating_Supply).HasColumnType("decimal(38,8)");
                entity.Property(c => c.Total_Supply).HasColumnType("decimal(38,8)");
                entity.Property(c => c.Max_Supply).HasColumnType("decimal(38,8)");

                // All-time high / low
                entity.Property(c => c.Ath).HasColumnType("decimal(38,18)");
                entity.Property(c => c.Atl).HasColumnType("decimal(38,18)");
            });

            // Alerts
            modelBuilder.Entity<PriceAlert>()
                .Property(a => a.ThresholdPrice)
                .HasColumnType("decimal(18,8)");

            modelBuilder.Entity<PriceAlert>()
                .Property(a => a.ThresholdPercent)
                .HasColumnType("decimal(18,4)");

            modelBuilder.Entity<PriceAlert>()
                .Property(a => a.AbovePrice)
                .HasColumnType("decimal(18,8)");

            modelBuilder.Entity<PriceAlert>()
                .Property(a => a.BelowPrice)
                .HasColumnType("decimal(18,8)");

            modelBuilder.Entity<PriceAlert>()
                .Property(a => a.AbovePercentFromBuy)
                .HasColumnType("decimal(18,4)");

            modelBuilder.Entity<PriceAlert>()
                .Property(a => a.BelowPercentFromBuy)
                .HasColumnType("decimal(18,4)");

            modelBuilder.Entity<PriceAlert>()
                .Property(a => a.BalanceBelow)
                .HasColumnType("decimal(18,8)");

            modelBuilder.Entity<PriceAlert>()
                .HasIndex(a => new { a.UserId, a.IsActive });

            modelBuilder.Entity<PriceAlert>()
                .HasIndex(a => new { a.CoinId, a.IsActive });
        }

    }
}
