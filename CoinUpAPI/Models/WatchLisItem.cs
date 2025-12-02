using CoinUp.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoinUpAPI.Models
{
    public class WatchlistItem
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        [Required]
        public string CoinId { get; set; }

        [ForeignKey("CoinId")]
        public CoinsMarket Coin { get; set; } = null!;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }

}
