using CoinUpAPI.Dto;
using CoinUpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoinUpAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AlertsController : ControllerBase
    {
        private readonly IAlertsService _alerts;

        public AlertsController(IAlertsService alerts)
        {
            _alerts = alerts;
        }

        private string? GetUserId()
            => User.FindFirst(ClaimTypes.NameIdentifier)?.Value
               ?? User.FindFirst("sub")?.Value;

        [HttpGet]
        public async Task<IActionResult> GetMyAlerts()
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "Invalid or missing user ID in token." });

            return Ok(await _alerts.GetMyAlertsAsync(userId));
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePriceAlertDto dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "Invalid or missing user ID in token." });

            try
            {
                var created = await _alerts.CreateAsync(userId, dto);
                return Ok(created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdatePriceAlertDto dto)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "Invalid or missing user ID in token." });

            try
            {
                var updated = await _alerts.UpdateAsync(userId, id, dto);
                return Ok(updated);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var userId = GetUserId();
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "Invalid or missing user ID in token." });

            try
            {
                await _alerts.DeleteAsync(userId, id);
                return Ok(new { message = "Deleted" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
