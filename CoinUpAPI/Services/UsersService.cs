using CoinUpAPI.Data;
using CoinUpAPI.Dto;
using Microsoft.EntityFrameworkCore;

namespace CoinUpAPI.Services
{
    public interface IUsersService
    {
        Task<IReadOnlyList<UserAdminListItemDto>> GetUsersAsync();
        Task SetUserIsActiveAsync(string userId, bool isActive);
    }

    public sealed class UsersService : IUsersService
    {
        private readonly ApplicationDbContext _db;

        public UsersService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<UserAdminListItemDto>> GetUsersAsync()
        {
            return await _db.Users
                .AsNoTracking()
                .Where(u => u.Role != "Admin")
                .Select(u => new UserAdminListItemDto
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();
        }

        public async Task SetUserIsActiveAsync(string userId, bool isActive)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                throw new KeyNotFoundException("User not found");

            user.IsActive = isActive;
            await _db.SaveChangesAsync();
        }
    }
}
