using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoinUpAPI.Models
{
    public class PortfolioSnapshot
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public decimal TotalValue { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
