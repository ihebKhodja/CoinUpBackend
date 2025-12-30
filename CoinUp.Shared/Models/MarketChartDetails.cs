using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace CoinUp.Shared.Models
{
    public class MarketChartDetails
    {
        [Key]
        // Coin id from CoinGecko (ex: "bitcoin").
        // We keep this as the primary key so there is one DB row per coin.
        public string Id { get; set; } = string.Empty;
        public int Rank { get; set; }

        // JSON stored in DB. Contains multiple time windows keyed by days (1/7/30/90/365).
        // Example:
        // { "1": {"prices": [[...]], ... }, "7": { ... }, "365": { ... } }
        public string ChartsJson { get; set; } = "{}";

        // Runtime helper (NOT mapped): read/write the JSON as a dictionary.
        // IMPORTANT: if you mutate the returned dictionary, re-assign it back to persist to ChartsJson.
        [NotMapped]
        public Dictionary<int, MarketChartWindow> Charts
        {
            get => string.IsNullOrWhiteSpace(ChartsJson)
                ? new()
                : JsonSerializer.Deserialize<Dictionary<int, MarketChartWindow>>(ChartsJson) ?? new();
            set => ChartsJson = JsonSerializer.Serialize(value);
        }

    }

    public class MarketChartWindow
    {
        public List<List<decimal>> Prices { get; set; } = new();
        public List<List<decimal>> MarketCaps { get; set; } = new();
        public List<List<decimal>> TotalVolumes { get; set; } = new();
    }

}
