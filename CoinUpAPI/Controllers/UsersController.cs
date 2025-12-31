using CoinUpAPI.Data;
using CoinUpAPI.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoinUpAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [AdminOnly]
    public class UsersController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public UsersController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _db.Users
                .AsNoTracking()
                .Select(u => new
                {
                    u.Id,
                    u.Username,
                    u.Email,
                    u.Role,
                    u.CreatedAt
                })
                .ToListAsync();

            return Ok(users);
        }
    }
}
