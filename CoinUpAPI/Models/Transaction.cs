using CoinUp.Shared.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoinUpAPI.Models
{
    public class Transaction
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

        [Required]
        public TransactionType Type { get; set; }

        public decimal Quantity { get; set; }

        public decimal PriceAtOperation { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum TransactionType
    {
        Buy,
        Sell
    }
}
