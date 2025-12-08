using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoinUpAPI.Models
{
    public class EWallet
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;

        public decimal Balance { get; set; } = 0;

        // Navigation
        public ICollection<CoinHolding> Holdings { get; set; } = new List<CoinHolding>();
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}
