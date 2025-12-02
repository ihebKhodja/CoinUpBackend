using CoinUp.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoinUpAPI.Models
{
    public class CoinHolding
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        public string WalletId { get; set; }

        [ForeignKey("WalletId")]
        public EWallet Wallet { get; set; } = null!;

        [Required]
        public string CoinId { get; set; }

        [ForeignKey("CoinId")]
        public CoinsMarket Coin { get; set; } = null!;

        public decimal Quantity { get; set; }
        public decimal AverageBuyPrice { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public decimal CurrentValue { get; set; }

        [NotMapped]
        public decimal Profit { get; set; }
    }
}
