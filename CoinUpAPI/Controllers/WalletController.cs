using CoinUpAPI.Dto;
using CoinUpAPI.Models;
using CoinUpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CoinUpAPI.Controllers
{
    [ApiController]
    [Route("api/wallet")]
    [Authorize]
    public class WalletController : ControllerBase
    {
        private readonly IWalletService _wallet;

        public WalletController(IWalletService wallet)
        {
            _wallet = wallet;
        }

        [HttpGet]
        public async Task<IActionResult> GetWallet()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? User.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Invalid or missing user ID in token." });

                var wallet = await _wallet.GetWalletAsync(userId);
                return Ok(wallet);
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



        [HttpPost("buy")]
        public async Task<IActionResult> Buy([FromBody] BuySellRequestDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid or missing user ID in token." });
            ;
            return Ok(await _wallet.BuyAsync(userId, dto));
        }

        [HttpPost("sell")]
        public async Task<IActionResult> Sell([FromBody] BuySellRequestDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid or missing user ID in token." });

            return Ok(await _wallet.SellAsync(userId, dto));
        }

        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                                        ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "Invalid or missing user ID in token." });
            return Ok(await _wallet.GetTransactionsAsync(userId));
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] DepositRequestDto dto)
        {
            if (dto == null || dto.Amount <= 0)
                return BadRequest(new { message = "Amount must be greater than 0." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst("sub")?.Value;

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized(new { message = "Invalid or missing user ID in token." });

            try
            {
                var success = await _wallet.DepositAsync(userId, dto.Amount);
                if (!success)
                    return BadRequest(new { message = "Deposit failed." });

                return Ok(new { message = "Deposit successful.", amount = dto.Amount });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Server error: " + ex.Message });
            }
        }

    }

}
