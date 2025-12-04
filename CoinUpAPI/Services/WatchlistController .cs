using CoinUpAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoinUpAPI.Services
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
            var userId = User.FindFirst("id").Value;
            await _watchlist.AddAsync(userId, coinId);
            return Ok();
        }

        [HttpDelete("{coinId}")]
        public async Task<IActionResult> Remove(string coinId)
        {
            var userId = User.FindFirst("id").Value;
            await _watchlist.RemoveAsync(userId, coinId);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var userId = User.FindFirst("id").Value;
            return Ok(await _watchlist.GetAsync(userId));
        }
    }

}
