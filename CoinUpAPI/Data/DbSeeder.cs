using CoinUpAPI.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace CoinUpAPI.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAdminAsync(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

            var email = config["SeedAdmin:Email"] ?? "admin@coinup.local";
            var username = config["SeedAdmin:Username"] ?? "admin";
            var password = config["SeedAdmin:Password"];

            if (string.IsNullOrWhiteSpace(password))
            {
                if (env.IsDevelopment())
                {
                    password = "Admin123!";
                }
                else
                {
                    logger.LogWarning("SeedAdmin:Password not set. Skipping admin seeding in non-development.");
                    return;
                }
            }

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    Username = username,
                    Role = "Admin"
                };
                user.PasswordHash = hasher.HashPassword(user, password);

                db.Users.Add(user);
                await db.SaveChangesAsync();

                logger.LogInformation("Seeded admin user: {Email} / {Username}", email, username);
            }
            else
            {
                var changed = false;

                if (!string.Equals(user.Role, "Admin", StringComparison.OrdinalIgnoreCase))
                {
                    user.Role = "Admin";
                    changed = true;
                }

                if (!string.Equals(user.Username, username, StringComparison.Ordinal))
                {
                    user.Username = username;
                    changed = true;
                }

                if (changed)
                {
                    await db.SaveChangesAsync();
                }

                logger.LogInformation("Admin user already exists: {Email}", email);
            }

            logger.LogInformation("Admin credentials (dev): Email={Email} Password={Password}", email, password);
        }
    }
}
