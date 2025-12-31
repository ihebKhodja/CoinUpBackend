using CoinUp.Shared.Models;
using CoinUpAPI.Data;
using CoinUpAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace CoinUpAPI.Tests.TestHelpers;

public static class TestDb
{
    public static DbContextOptions<ApplicationDbContext> CreateOptions(string databaseName)
        => new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName)
            .EnableSensitiveDataLogging()
            .Options;

    public static async Task SeedUserWithWalletAsync(
        ApplicationDbContext db,
        string userId,
        string email,
        decimal balance)
    {
        var user = new User
        {
            Id = userId,
            Username = "test",
            Email = email,
            PasswordHash = "hash",
            Role = "User",
        };

        var wallet = new EWallet
        {
            Id = $"wallet-{userId}",
            UserId = userId,
            User = user,
            Balance = balance,
        };

        user.Wallet = wallet;

        db.Users.Add(user);
        db.EWallets.Add(wallet);

        await db.SaveChangesAsync();
    }

    public static async Task SeedCoinAsync(ApplicationDbContext db, string coinId, decimal currentPrice)
    {
        db.CoinsMarket.Add(new CoinsMarket
        {
            Id = coinId,
            Name = "Bitcoin",
            Symbol = "BTC",
            Rank = 1,
            Current_Price = currentPrice,
            Last_Updated = DateTime.UtcNow,
        });

        await db.SaveChangesAsync();
    }

    public static async Task SeedWatchlistAsync(ApplicationDbContext db, string userId, string coinId)
    {
        var user = await db.Users.SingleAsync(u => u.Id == userId);
        var coin = await db.CoinsMarket.SingleAsync(c => c.Id == coinId);

        db.WatchlistItems.Add(new WatchlistItem
        {
            UserId = userId,
            User = user,
            CoinId = coinId,
            Coin = coin,
        });

        await db.SaveChangesAsync();
    }

    public static async Task SeedHoldingAsync(ApplicationDbContext db, string userId, string coinId, decimal avgBuy)
    {
        var wallet = await db.EWallets.SingleAsync(w => w.UserId == userId);
        var coin = await db.CoinsMarket.SingleAsync(c => c.Id == coinId);

        db.CoinHoldings.Add(new CoinHolding
        {
            WalletId = wallet.Id,
            Wallet = wallet,
            CoinId = coinId,
            Coin = coin,
            Quantity = 1m,
            AverageBuyPrice = avgBuy,
        });

        await db.SaveChangesAsync();
    }
}
