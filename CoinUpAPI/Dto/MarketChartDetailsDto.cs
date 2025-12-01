namespace CoinUpAPI.Dto
{
    public class MarketChartDetailsDto
    {
        public string Id { get; set; } = string.Empty;

        public int Rank { get; set; }

        public List<List<decimal>> Prices { get; set; } = new();

        public List<List<decimal>> MarketCaps { get; set; } = new();

        public List<List<decimal>> TotalVolumes { get; set; } = new();
    }

}
