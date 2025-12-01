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
        public string Id { get; set; }
        public int Rank { get; set; }

        // JSON columns stored in database
        public string PricesJson { get; set; }
        public string MarketCapsJson { get; set; }
        public string TotalVolumesJson { get; set; }

        // Runtime properties (NOT mapped)
        [NotMapped]
        public List<List<decimal>> Prices
        {
            get => string.IsNullOrEmpty(PricesJson)
                ? new()
                : JsonSerializer.Deserialize<List<List<decimal>>>(PricesJson);
            set => PricesJson = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<List<decimal>> MarketCaps
        {
            get => string.IsNullOrEmpty(MarketCapsJson)
                ? new()
                : JsonSerializer.Deserialize<List<List<decimal>>>(MarketCapsJson);
            set => MarketCapsJson = JsonSerializer.Serialize(value);
        }

        [NotMapped]
        public List<List<decimal>> TotalVolumes
        {
            get => string.IsNullOrEmpty(TotalVolumesJson)
                ? new()
                : JsonSerializer.Deserialize<List<List<decimal>>>(TotalVolumesJson);
            set => TotalVolumesJson = JsonSerializer.Serialize(value);
        }
    }

}
