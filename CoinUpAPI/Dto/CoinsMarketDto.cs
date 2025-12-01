namespace CoinUpAPI.Dto
{
    public class CoinsMarketDto
    {
        public string Id { get; set; } = string.Empty;
        public string Symbol { get; set; } = string.Empty;
        public int Rank { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;

        public decimal CurrentPrice { get; set; }
        public decimal? MarketCap { get; set; }
        public int MarketCapRank { get; set; }
        public decimal? FullyDilutedValuation { get; set; }
        public decimal TotalVolume { get; set; }

        public decimal High24h { get; set; }
        public decimal Low24h { get; set; }
        public decimal PriceChange24h { get; set; }
        public decimal PriceChangePercentage24h { get; set; }
        public decimal MarketCapChange24h { get; set; }
        public decimal MarketCapChangePercentage24h { get; set; }

        public decimal CirculatingSupply { get; set; }
        public decimal? TotalSupply { get; set; }
        public decimal? MaxSupply { get; set; }

        public decimal Ath { get; set; }
        public decimal AthChangePercentage { get; set; }
        public DateTime AthDate { get; set; }

        public decimal Atl { get; set; }
        public decimal AtlChangePercentage { get; set; }
        public DateTime AtlDate { get; set; }

        public DateTime LastUpdated { get; set; }
    }

    public class PaginatedCoinsResponse
    {
        public int TotalItems { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public List<CoinsMarketDto> Items { get; set; }
    }


}
