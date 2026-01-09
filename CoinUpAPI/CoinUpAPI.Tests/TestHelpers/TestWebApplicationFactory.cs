using CoinUpAPI.Data;
using CoinUpAPI.Services.Alerts;
using CoinUpAPI.Services.Email;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CoinUpAPI.Tests.TestHelpers;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = Guid.NewGuid().ToString();

    public FakeEmailSender FakeEmail { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Prevent HTTP->HTTPS redirects in tests (Program.cs skips HTTPS redirection when this is true).
        Environment.SetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER", "true");

        builder.UseEnvironment("Development");

        builder.ConfigureServices(services =>
        {
            // Replace DbContext with InMemory
            var dbDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (dbDescriptor != null)
                services.Remove(dbDescriptor);

            services.AddDbContext<ApplicationDbContext>(o => o.UseInMemoryDatabase(_dbName));

            // Replace auth with a deterministic test scheme
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
            }).AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(TestAuthHandler.SchemeName, _ => { });

            // Replace email sender to avoid SMTP
            var emailDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IEmailSender));
            if (emailDescriptor != null)
                services.Remove(emailDescriptor);
            services.AddScoped<IEmailSender>(_ => FakeEmail);

            // Disable background evaluator in integration tests
            var hostedDescriptors = services
                .Where(d => d.ImplementationType == typeof(AlertEvaluationHostedService))
                .ToList();
            foreach (var d in hostedDescriptors)
                services.Remove(d);
        });
    }
}
