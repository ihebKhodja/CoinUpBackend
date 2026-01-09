using CoinUp.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace CoinUpWorkerService.Data
{
    internal class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
        public DbSet<CoinsMarket> CoinsMarket { get; set; }
        public DbSet<CoinsMarketCategory> CoinsMarketCategory { get; set; }
        public DbSet<MarketChartDetails> MarketChartDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

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

            var stringListConverter = new ValueConverter<List<string>, string>(
                v => SerializeStringList(v),
                v => DeserializeStringList(v)
            );

            var stringListComparer = new ValueComparer<List<string>>(
                (a, b) => a != null && b != null && a.SequenceEqual(b),
                v => v.Aggregate(0, (acc, x) => HashCode.Combine(acc, x.GetHashCode())),
                v => v.ToList()
            );

            modelBuilder.Entity<CoinsMarketCategory>(entity =>
            {
                entity.Property(e => e.Top3CoinsId)
                    .HasConversion(stringListConverter)
                    .Metadata.SetValueComparer(stringListComparer);

                entity.Property(e => e.Top3Coins)
                    .HasConversion(stringListConverter)
                    .Metadata.SetValueComparer(stringListComparer);
            });
        }

        private static string SerializeStringList(List<string> value)
        {
            return JsonSerializer.Serialize(value);
        }

        private static List<string> DeserializeStringList(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? new List<string>()
                : (JsonSerializer.Deserialize<List<string>>(value) ?? new List<string>());
        }

    }
}
