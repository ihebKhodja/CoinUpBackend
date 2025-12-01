using CoinUpAPI.Data;
using CoinUpAPI.Dto;
using CoinUpAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoinUpAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]



    public class CoinsController : ControllerBase
    {
        private readonly ICoinsService _coinsService;


        public CoinsController(ICoinsService coinsService)
        {
            _coinsService = coinsService;
        }

        // GET: api/coins
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedCoinsResponse))]
        [HttpGet]
        public async Task<IActionResult> GetAllCoins([FromQuery] string? query, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _coinsService.GetAllAsync(query, page, pageSize);
            return Ok(result);
        }


        // GET: api/coins/{id}
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CoinsMarketDto))]
        public async Task<IActionResult> GetCoinById(string id)
        {
            var coin = await _coinsService.GetByIdAsync(id);
            if (coin == null)
                return NotFound(new { Message = $"Coin with ID '{id}' not found." });

            return Ok(coin);
        }
    }


}
