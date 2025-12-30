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
