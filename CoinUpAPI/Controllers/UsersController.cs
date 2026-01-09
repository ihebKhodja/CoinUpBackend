using CoinUpAPI.Dto;
using CoinUpAPI.Security;
using CoinUpAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CoinUpAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [AdminOnly]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _usersService;

        public UsersController(IUsersService usersService)
        {
            _usersService = usersService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _usersService.GetUsersAsync();
            return Ok(users);
        }

        [HttpPut("{userId}/is-active")]
        public async Task<IActionResult> SetUserIsActive([FromRoute] string userId, [FromBody] SetUserActiveDto dto)
        {
            try
            {
                await _usersService.SetUserIsActiveAsync(userId, dto.IsActive);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
