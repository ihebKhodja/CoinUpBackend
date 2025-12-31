using System;
using System.ComponentModel.DataAnnotations;

namespace CoinUpAPI.Models;

public class AlertNotification
{
    [Key]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [Required]
    public string AlertId { get; set; } = string.Empty;

    [Required]
    public string UserId { get; set; } = string.Empty;

    [Required]
    public string Channel { get; set; } = "Email";

    public bool Success { get; set; }

    public string? Error { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
