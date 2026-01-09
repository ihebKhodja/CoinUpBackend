using CoinUpAPI.Data;
using CoinUpAPI.Dto;
using CoinUpAPI.Models;
using CoinUpAPI.Services;
using CoinUpAPI.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CoinUpAPI.Tests;

public class AlertsServiceTests
{
    [Fact]
    public async Task Create_WatchlistAlert_Throws_When_CoinNotInWatchlist()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = new ApplicationDbContext(TestDb.CreateOptions(dbName));

        await TestDb.SeedUserWithWalletAsync(db, userId: "u1", email: "u1@test.local", balance: 500m);
        await TestDb.SeedCoinAsync(db, coinId: "btc", currentPrice: 100m);

        var service = new AlertsService(db);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync("u1", new CreatePriceAlertDto
            {
                Type = AlertType.WatchlistPrice,
                CoinId = "btc",
                AbovePrice = 90m,
                IsActive = true,
                CooldownMinutes = 60
            }));

        Assert.Contains("watchlist", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_WatchlistAlert_Creates_When_InWatchlist_And_ThresholdValid()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = new ApplicationDbContext(TestDb.CreateOptions(dbName));

        await TestDb.SeedUserWithWalletAsync(db, userId: "u1", email: "u1@test.local", balance: 500m);
        await TestDb.SeedCoinAsync(db, coinId: "btc", currentPrice: 100m);
        await TestDb.SeedWatchlistAsync(db, userId: "u1", coinId: "btc");

        var service = new AlertsService(db);

        var created = await service.CreateAsync("u1", new CreatePriceAlertDto
        {
            Type = AlertType.WatchlistPrice,
            CoinId = "btc",
            AbovePrice = 90m,
            IsActive = true,
            CooldownMinutes = 10
        });

        Assert.False(string.IsNullOrWhiteSpace(created.Id));
        Assert.Equal("btc", created.CoinId);
        Assert.Equal(AlertType.WatchlistPrice, created.Type);
        Assert.Equal(90m, created.AbovePrice);

        var saved = await db.PriceAlerts.SingleAsync(a => a.Id == created.Id);
        Assert.Equal("u1", saved.UserId);
        Assert.True(saved.IsActive);
    }

    [Fact]
    public async Task Create_WalletPriceVsBuy_Throws_When_CoinNotInHoldings()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = new ApplicationDbContext(TestDb.CreateOptions(dbName));

        await TestDb.SeedUserWithWalletAsync(db, userId: "u1", email: "u1@test.local", balance: 500m);
        await TestDb.SeedCoinAsync(db, coinId: "btc", currentPrice: 100m);

        var service = new AlertsService(db);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync("u1", new CreatePriceAlertDto
            {
                Type = AlertType.WalletPriceVsBuy,
                CoinId = "btc",
                AbovePercentFromBuy = 10m,
                IsActive = true
            }));

        Assert.Contains("holdings", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_WalletPriceVsBuy_Creates_When_InHoldings()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = new ApplicationDbContext(TestDb.CreateOptions(dbName));

        await TestDb.SeedUserWithWalletAsync(db, userId: "u1", email: "u1@test.local", balance: 500m);
        await TestDb.SeedCoinAsync(db, coinId: "btc", currentPrice: 100m);
        await TestDb.SeedHoldingAsync(db, userId: "u1", coinId: "btc", avgBuy: 80m);

        var service = new AlertsService(db);

        var created = await service.CreateAsync("u1", new CreatePriceAlertDto
        {
            Type = AlertType.WalletPriceVsBuy,
            CoinId = "btc",
            AbovePercentFromBuy = 10m,
            IsActive = true
        });

        Assert.Equal(AlertType.WalletPriceVsBuy, created.Type);
        Assert.Equal("btc", created.CoinId);
        Assert.Equal(10m, created.AbovePercentFromBuy);
    }

    [Fact]
    public async Task Create_WalletBalanceBelow_Throws_When_BalanceBelowMissing()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = new ApplicationDbContext(TestDb.CreateOptions(dbName));

        await TestDb.SeedUserWithWalletAsync(db, userId: "u1", email: "u1@test.local", balance: 50m);

        var service = new AlertsService(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync("u1", new CreatePriceAlertDto
            {
                Type = AlertType.WalletBalanceBelow,
                BalanceBelow = null,
                IsActive = true
            }));
    }

    [Fact]
    public async Task Create_WalletBalanceBelow_Allows_Null_CoinId()
    {
        var dbName = Guid.NewGuid().ToString();
        await using var db = new ApplicationDbContext(TestDb.CreateOptions(dbName));

        await TestDb.SeedUserWithWalletAsync(db, userId: "u1", email: "u1@test.local", balance: 50m);

        var service = new AlertsService(db);

        var created = await service.CreateAsync("u1", new CreatePriceAlertDto
        {
            Type = AlertType.WalletBalanceBelow,
            CoinId = null,
            BalanceBelow = 100m,
            IsActive = true
        });

        Assert.Equal(AlertType.WalletBalanceBelow, created.Type);
        Assert.Null(created.CoinId);
        Assert.Equal(100m, created.BalanceBelow);
    }
}
