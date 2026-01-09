using System.Net;
using System.Net.Http.Json;
using CoinUpAPI.Dto;
using CoinUpAPI.Tests.TestHelpers;
using Xunit;

namespace CoinUpAPI.Tests;

public class AuthControllerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public AuthControllerIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_Returns200_AndToken()
    {
        var client = _factory.CreateClient();

        var res = await client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Username = "tester",
            Email = $"tester-{Guid.NewGuid()}@test.local",
            Password = "Password123!"
        });

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var payload = await res.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.Token));
        Assert.False(string.IsNullOrWhiteSpace(payload.UserId));
        Assert.Equal("tester", payload.Username);
    }

    [Fact]
    public async Task Login_Returns200_AndToken()
    {
        var client = _factory.CreateClient();

        var email = $"login-{Guid.NewGuid()}@test.local";
        var password = "Password123!";

        var reg = await client.PostAsJsonAsync("/api/auth/register", new RegisterDto
        {
            Username = "login-user",
            Email = email,
            Password = password
        });
        Assert.Equal(HttpStatusCode.OK, reg.StatusCode);

        var res = await client.PostAsJsonAsync("/api/auth/login", new LoginDto
        {
            Email = email,
            Password = password
        });

        Assert.Equal(HttpStatusCode.OK, res.StatusCode);

        var payload = await res.Content.ReadFromJsonAsync<AuthResponseDto>();
        Assert.NotNull(payload);
        Assert.False(string.IsNullOrWhiteSpace(payload!.Token));
        Assert.False(string.IsNullOrWhiteSpace(payload.UserId));
        Assert.Equal(email, payload.Email);
    }
}
