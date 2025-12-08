namespace CoinUpAPI.Dto
{
    public class BuySellRequestDto
    {
        public string CoinId { get; set; }             // CoinsMarket Id
        public decimal Quantity { get; set; }          // How much to buy/sell
    }

}
