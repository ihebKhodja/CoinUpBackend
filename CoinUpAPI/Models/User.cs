using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace CoinUpAPI.Models;

public class User
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string Username { get; set; } = string.Empty;

    [Required]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [Required]
    public string Role { get; set; } = "User";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public EWallet Wallet { get; set; } = new();
    public ICollection<WatchlistItem> Watchlist { get; set; } = new List<WatchlistItem>();
    public ICollection<PortfolioSnapshot> PortfolioSnapshots { get; set; } = new List<PortfolioSnapshot>();
}
