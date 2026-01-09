using System;
using System.ComponentModel.DataAnnotations;

namespace CoinUpAPI.Models;

public enum AlertType
{
    WatchlistPrice = 1,
    WalletPriceVsBuy = 2,
    WalletBalanceBelow = 3
}

public class PriceAlert
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string UserId { get; set; } = string.Empty;

    // Required for coin-based alerts; null for WalletBalanceBelow.
    public string? CoinId { get; set; }

    [Required]
    public AlertType Type { get; set; }

    // Absolute price thresholds (used by WatchlistPrice)
    public decimal? AbovePrice { get; set; }
    public decimal? BelowPrice { get; set; }

    // Percent thresholds relative to the user's AverageBuyPrice (used by WalletPriceVsBuy)
    // Example: AbovePercentFromBuy = 10 triggers when currentPrice >= avgBuy * 1.10
    public decimal? AbovePercentFromBuy { get; set; }
    // Example: BelowPercentFromBuy = 5 triggers when currentPrice <= avgBuy * 0.95
    public decimal? BelowPercentFromBuy { get; set; }

    // Wallet balance threshold (used by WalletBalanceBelow)
    public decimal? BalanceBelow { get; set; }

    // Legacy fields (kept for backwards compatibility)
    public decimal? ThresholdPrice { get; set; }
    public decimal? ThresholdPercent { get; set; }

    public bool IsActive { get; set; } = true;

    public int CooldownMinutes { get; set; } = 60;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastTriggeredAt { get; set; }
}
