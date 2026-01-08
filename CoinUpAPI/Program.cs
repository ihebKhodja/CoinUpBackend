using CoinUpAPI.Data;
using CoinUpAPI.Config;
using CoinUpAPI.Models;
using CoinUpAPI.Middleware;
using CoinUpAPI.Services;
using CoinUpAPI.Services.Alerts;
using CoinUpAPI.Services.Email;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;


// Load .env before configuration is built
DotEnv.Load(Path.Combine(AppContext.BaseDirectory, ".env"));
DotEnv.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:4200",
                "https://localhost:4200"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddScoped<IUsersService, UsersService>();

builder.Services.AddScoped<ICoinsService, CoinsService>();
builder.Services.AddScoped<IWalletService, WalletService>();
builder.Services.AddScoped<IWatchlistService, WatchlistService>();
builder.Services.AddScoped<IAlertsService, AlertsService>();

builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection("Smtp"));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddHostedService<AlertEvaluationHostedService>();

// JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

// Add services to the container.
builder.Services.AddControllers();

// Configure EF Core
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Coin API",
        Version = "1.0.0",               // ✅ Use valid semantic version
        Description = "API for retrieving coin market data"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token like: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new string[] { }
        }
    });
});

var app = builder.Build();

bool? runMigrationsSetting = builder.Configuration["RUN_MIGRATIONS"]?.Trim() switch
{
    "true" or "TRUE" or "True" => true,
    "false" or "FALSE" or "False" => false,
    _ => null
};

var runMigrations = runMigrationsSetting ?? builder.Environment.IsDevelopment();

if (runMigrations)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (!db.Database.IsRelational())
    {
        app.Logger.LogInformation("Skipping database migrations because the configured EF provider is not relational.");
    }
    else
    {

        const int maxAttempts = 30;
        Exception? lastMigrationError = null;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                await db.Database.MigrateAsync();
                lastMigrationError = null;
                break;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                lastMigrationError = ex;
                app.Logger.LogWarning(ex, "Database migration attempt {Attempt}/{MaxAttempts} failed; retrying...", attempt, maxAttempts);
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
            catch (Exception ex)
            {
                lastMigrationError = ex;
                app.Logger.LogError(ex, "Database migration failed after {MaxAttempts} attempts.", maxAttempts);
                throw;
            }
        }

        if (lastMigrationError is not null)
        {
            app.Logger.LogError(lastMigrationError, "Database migration failed after {MaxAttempts} attempts.", maxAttempts);
            throw lastMigrationError;
        }
    }
}

// Seed admin user (dev-friendly defaults)
await app.SeedAdminAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Coin API v1");
        //c.RoutePrefix = string.Empty; // optional: Swagger UI at root
    });
}

if (app.Environment.IsDevelopment())
{
    app.UseCors("Frontend");
}

var runningInContainer = string.Equals(
    Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER"),
    "true",
    StringComparison.OrdinalIgnoreCase);

if (!runningInContainer)
{
    app.UseHttpsRedirection();
}
app.UseAuthentication();
app.UseMiddleware<AdminOnlyMiddleware>();
app.UseAuthorization();
app.MapControllers();

//app.Urls.Add("http://0.0.0.0:8080");
app.Run();

public partial class Program { }
