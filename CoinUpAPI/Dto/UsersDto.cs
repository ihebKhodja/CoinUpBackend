using System;
using System.ComponentModel.DataAnnotations;

namespace CoinUpAPI.Dto
{
    public sealed class UserAdminListItemDto
    {
        public string Id { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public sealed class SetUserActiveDto
    {
        [Required]
        public bool IsActive { get; set; }
    }
}
