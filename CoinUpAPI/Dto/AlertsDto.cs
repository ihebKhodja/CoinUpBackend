using System;
using System.ComponentModel.DataAnnotations;
using CoinUpAPI.Models;

namespace CoinUpAPI.Dto
{
    public class CreatePriceAlertDto
    {
        // Required for coin-based alerts; null/empty for WalletBalanceBelow.
        public string? CoinId { get; set; }

        [Required]
        public AlertType Type { get; set; }

        // WatchlistPrice
        public decimal? AbovePrice { get; set; }
        public decimal? BelowPrice { get; set; }

        // WalletPriceVsBuy
        public decimal? AbovePercentFromBuy { get; set; }
        public decimal? BelowPercentFromBuy { get; set; }

        // WalletBalanceBelow
        public decimal? BalanceBelow { get; set; }

        public bool IsActive { get; set; } = true;

        public int CooldownMinutes { get; set; } = 60;
    }

    public class UpdatePriceAlertDto
    {
        public bool? IsActive { get; set; }

        // WatchlistPrice
        public decimal? AbovePrice { get; set; }
        public decimal? BelowPrice { get; set; }

        // WalletPriceVsBuy
        public decimal? AbovePercentFromBuy { get; set; }
        public decimal? BelowPercentFromBuy { get; set; }

        // WalletBalanceBelow
        public decimal? BalanceBelow { get; set; }

        public int? CooldownMinutes { get; set; }
        public AlertType? Type { get; set; }
    }

    public class PriceAlertDto
    {
        public string Id { get; set; } = string.Empty;
        public string? CoinId { get; set; }
        public AlertType Type { get; set; }

        public decimal? AbovePrice { get; set; }
        public decimal? BelowPrice { get; set; }
        public decimal? AbovePercentFromBuy { get; set; }
        public decimal? BelowPercentFromBuy { get; set; }
        public decimal? BalanceBelow { get; set; }

        public bool IsActive { get; set; }
        public int CooldownMinutes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastTriggeredAt { get; set; }
    }
}
