using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CoinUpAPI.Data;
using CoinUpAPI.Dto;
using CoinUpAPI.Models;
using CoinUpAPI.Tests.TestHelpers;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CoinUpAPI.Tests;

public class WalletControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public WalletControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private static async Task SeedUserAsync(ApplicationDbContext db, string userId)
    {
        if (await db.Users.FindAsync(userId) != null)
            return;

        db.Users.Add(new User
        {
            Id = userId,
            Username = "test",
            Email = "u1@test.local",
            PasswordHash = "hash",
            Role = "User",
            IsActive = true
        });

        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task GetWallet_Returns200_AndCreatesWallet_WhenMissing()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await SeedUserAsync(db, userId: "u1");
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        var res = await client.GetAsync("/api/wallet");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var payload = await res.Content.ReadFromJsonAsync<EWalletDto>();
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.WalletId));
        Assert.Equal(0m, payload.Balance);
        Assert.NotNull(payload.Holdings);
    }

    [Fact]
    public async Task Deposit_Returns400_WhenAmountInvalid()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        var res = await client.PostAsJsonAsync("/api/wallet/deposit", new DepositRequestDto
        {
            Amount = 0m
        });

        Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
    }

    [Fact]
    public async Task Deposit_Returns200_AndUpdatesBalance()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await SeedUserAsync(db, userId: "u1");
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        var deposit = await client.PostAsJsonAsync("/api/wallet/deposit", new DepositRequestDto
        {
            Amount = 50m
        });
        Assert.Equal(HttpStatusCode.OK, deposit.StatusCode);

        var walletRes = await client.GetAsync("/api/wallet");
        Assert.Equal(HttpStatusCode.OK, walletRes.StatusCode);

        var wallet = await walletRes.Content.ReadFromJsonAsync<EWalletDto>();
        Assert.NotNull(wallet);
        Assert.Equal(50m, wallet!.Balance);
    }

    [Fact]
    public async Task GetTransactions_Returns200_AndEmptyList_WhenNoWallet()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await SeedUserAsync(db, userId: "u1");
        }

        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Test");

        var res = await client.GetAsync("/api/wallet/transactions");

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var payload = await res.Content.ReadFromJsonAsync<List<WalletTransactionDto>>();
        Assert.NotNull(payload);
        Assert.Empty(payload!);
    }
}
