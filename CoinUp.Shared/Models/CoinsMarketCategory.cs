using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinUp.Shared.Models
{
    public class CoinsMarketCategory
    {
        [Required]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;

        public double MarketCap { get; set; }
        public double MarketCapChange24h { get; set; }
        public string Content { get; set; } = string.Empty;

        public List<string> Top3CoinsId { get; set; } = new();
        public List<string> Top3Coins { get; set; } = new();

        public double Volume24h { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
