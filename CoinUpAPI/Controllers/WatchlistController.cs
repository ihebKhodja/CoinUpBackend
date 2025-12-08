using CoinUpAPI.Models;
using CoinUpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoinUpAPI.Controllers
{
    [ApiController]
    [Route("api/watchlist")]
    [Authorize]
    public class WatchlistController : ControllerBase
    {
        private readonly IWatchlistService _watchlist;

        public WatchlistController(IWatchlistService watchlist)
        {
            _watchlist = watchlist;
        }

        [HttpPost("{coinId}")]
        public async Task<IActionResult> Add(string coinId)
        {

            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? User.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Invalid or missing user ID in token." });

                var item = await _watchlist.AddAsync(userId, coinId);

                return CreatedAtAction(nameof(Get), new { }, item);

            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error.", details = ex.Message });
            }
        }

        [HttpDelete("{coinId}")]
        public async Task<IActionResult> Remove(string coinId)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Invalid or missing user ID in token." });

                await _watchlist.RemoveAsync(userId, coinId);
                return Ok(new { message = "Removed from watchlist." });

            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error.", details = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Invalid or missing user ID in token." });

                var result = await _watchlist.GetAsync(userId);
                return Ok(result);

            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Unexpected error.", details = ex.Message });
            }
        }
    }
}




