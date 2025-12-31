using CoinUpAPI.Security;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CoinUpAPI.Middleware
{
    public sealed class AdminOnlyMiddleware
    {
        private readonly RequestDelegate _next;

        public AdminOnlyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var endpoint = context.GetEndpoint();
            var requiresAdmin = endpoint?.Metadata.GetMetadata<AdminOnlyAttribute>() != null;

            if (!requiresAdmin)
            {
                await _next(context);
                return;
            }

            if (context.User?.Identity?.IsAuthenticated != true)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Unauthorized" });
                return;
            }

            var role = context.User.FindFirstValue(ClaimTypes.Role)
                       ?? context.User.Claims.FirstOrDefault(c => string.Equals(c.Type, "role", StringComparison.OrdinalIgnoreCase))?.Value;

            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { message = "Forbidden" });
                return;
            }

            await _next(context);
        }
    }
}
