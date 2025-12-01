namespace CoinUpAPI.Dto
{
    public class CoinsMarketCategoryDto
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public decimal MarketCap { get; set; }

        public decimal MarketCapChange24h { get; set; }

        public string Content { get; set; } = string.Empty;

        public List<string> Top3CoinsId { get; set; } = new();

        public List<string> Top3Coins { get; set; } = new();

        public decimal Volume24h { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
