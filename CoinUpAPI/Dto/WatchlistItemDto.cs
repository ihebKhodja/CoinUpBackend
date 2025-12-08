namespace CoinUpAPI.Dto
{
    public class WatchlistItemDto
    {
        public string Id { get; set; }
        public string CoinId { get; set; }
        public string CoinName { get; set; }
        public string Symbol { get; set; }
        public string Image { get; set; }
        public decimal CurrentPrice { get; set; }
        public DateTime AddedAt { get; set; }

    }

}
