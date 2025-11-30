using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinUp.Shared.Models
{
    public class CoinsMarket
    {
        public string Id { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public int Rank { get; set; }


        [Required]
        public string Name { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;

        public decimal Current_Price { get; set; }
        public decimal? Market_Cap { get; set; }
        public int Market_Cap_Rank { get; set; }
        public decimal? Fully_Diluted_Valuation { get; set; }
        public decimal Total_Volume { get; set; }
        public decimal High_24h { get; set; }
        public decimal Low_24h { get; set; }
        public decimal Price_Change_24h { get; set; }
        public decimal Price_Change_Percentage_24h { get; set; }
        public decimal Market_Cap_Change_24h { get; set; }
        public decimal Market_Cap_Change_Percentage_24h { get; set; }

        public decimal Circulating_Supply { get; set; }
        public decimal? Total_Supply { get; set; }
        public decimal? Max_Supply { get; set; }

        public decimal Ath { get; set; }
        public decimal Ath_Change_Percentage { get; set; }
        public DateTime Ath_Date { get; set; }

        public decimal Atl { get; set; }
        public decimal Atl_Change_Percentage { get; set; }
        public DateTime Atl_Date { get; set; }

        [NotMapped]
        public object? Roi { get; set; }
        public DateTime Last_Updated { get; set; }

    }
}
