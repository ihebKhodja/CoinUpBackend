using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinUp.Shared.Models
{
    public class CoinsMarketCategory
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double MarketCap { get; set; }
        public double MarketCapChange24h { get; set; }
        public string Content { get; set; }

        public List<string> Top3CoinsId { get; set; }
        public List<string> Top3Coins { get; set; }

        public double Volume24h { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
