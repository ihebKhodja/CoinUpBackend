using System.Reflection;
using CoinUpAPI.Data;
using CoinUpAPI.Models;
using CoinUpAPI.Services.Alerts;
using CoinUpAPI.Services.Email;
using CoinUpAPI.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace CoinUpAPI.Tests;

public class AlertEvaluationHostedServiceTests
{
    [Fact]
    public async Task EvaluateOnce_SendsEmail_And_WritesNotification_And_SetsLastTriggeredAt_WatchlistPrice()
    {
        var dbName = Guid.NewGuid().ToString();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(dbName));

        var fakeEmail = new FakeEmailSender();
        services.AddScoped<IEmailSender>(_ => fakeEmail);

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AlertEvaluation:IntervalSeconds"] = "60"
            })
            .Build();

        var serviceProvider = services.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await TestDb.SeedUserWithWalletAsync(db, userId: "u1", email: "u1@test.local", balance: 500m);
            await TestDb.SeedCoinAsync(db, coinId: "btc", currentPrice: 100m);
            await TestDb.SeedWatchlistAsync(db, userId: "u1", coinId: "btc");

            db.PriceAlerts.Add(new PriceAlert
            {
                UserId = "u1",
                Type = AlertType.WatchlistPrice,
                CoinId = "btc",
                AbovePrice = 90m,
                IsActive = true,
                CooldownMinutes = 60
            });

            await db.SaveChangesAsync();
        }

        var logger = serviceProvider.GetRequiredService<ILogger<AlertEvaluationHostedService>>();
        var hosted = new AlertEvaluationHostedService(serviceProvider, logger, config);

        await InvokeEvaluateOnce(hosted, CancellationToken.None);

        Assert.Single(fakeEmail.Sent);
        Assert.Contains("Watchlist", fakeEmail.Sent[0].Subject, StringComparison.OrdinalIgnoreCase);

        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var alert = await db.PriceAlerts.SingleAsync();
            Assert.True(alert.LastTriggeredAt.HasValue);

            var notif = await db.AlertNotifications.SingleAsync();
            Assert.True(notif.Success);
            Assert.Equal("u1", notif.UserId);
        }
    }

    [Fact]
    public async Task EvaluateOnce_RespectsCooldown_DoesNotResend()
    {
        var dbName = Guid.NewGuid().ToString();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(dbName));

        var fakeEmail = new FakeEmailSender();
        services.AddScoped<IEmailSender>(_ => fakeEmail);

        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var serviceProvider = services.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await TestDb.SeedUserWithWalletAsync(db, userId: "u1", email: "u1@test.local", balance: 500m);
            await TestDb.SeedCoinAsync(db, coinId: "btc", currentPrice: 100m);
            await TestDb.SeedWatchlistAsync(db, userId: "u1", coinId: "btc");

            db.PriceAlerts.Add(new PriceAlert
            {
                UserId = "u1",
                Type = AlertType.WatchlistPrice,
                CoinId = "btc",
                AbovePrice = 90m,
                IsActive = true,
                CooldownMinutes = 999,
                LastTriggeredAt = DateTime.UtcNow
            });

            await db.SaveChangesAsync();
        }

        var logger = serviceProvider.GetRequiredService<ILogger<AlertEvaluationHostedService>>();
        var hosted = new AlertEvaluationHostedService(serviceProvider, logger, config);

        await InvokeEvaluateOnce(hosted, CancellationToken.None);

        Assert.Empty(fakeEmail.Sent);

        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            Assert.Empty(await db.AlertNotifications.ToListAsync());
        }
    }

    [Fact]
    public async Task EvaluateOnce_SendsEmail_For_WalletBalanceBelow_When_BalanceIsBelowThreshold()
    {
        var dbName = Guid.NewGuid().ToString();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(dbName));

        var fakeEmail = new FakeEmailSender();
        services.AddScoped<IEmailSender>(_ => fakeEmail);

        var config = new ConfigurationBuilder().AddInMemoryCollection().Build();
        var serviceProvider = services.BuildServiceProvider();

        using (var scope = serviceProvider.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            await TestDb.SeedUserWithWalletAsync(db, userId: "u1", email: "u1@test.local", balance: 25m);

            db.PriceAlerts.Add(new PriceAlert
            {
                UserId = "u1",
                Type = AlertType.WalletBalanceBelow,
                BalanceBelow = 100m,
                IsActive = true,
                CooldownMinutes = 60
            });

            await db.SaveChangesAsync();
        }

        var logger = serviceProvider.GetRequiredService<ILogger<AlertEvaluationHostedService>>();
        var hosted = new AlertEvaluationHostedService(serviceProvider, logger, config);

        await InvokeEvaluateOnce(hosted, CancellationToken.None);

        Assert.Single(fakeEmail.Sent);
        Assert.Contains("Solde", fakeEmail.Sent[0].Subject, StringComparison.OrdinalIgnoreCase);
    }

    // Uses reflection because EvaluateOnce is private; keeps production code unchanged.
    private static async Task InvokeEvaluateOnce(AlertEvaluationHostedService hosted, CancellationToken token)
    {
        var method = typeof(AlertEvaluationHostedService)
            .GetMethod("EvaluateOnce", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(method);

        var task = (Task?)method!.Invoke(hosted, new object[] { token });
        Assert.NotNull(task);
        await task!;
    }
}
