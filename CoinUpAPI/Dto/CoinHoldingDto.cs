namespace CoinUpAPI.Dto
{
    public class CoinHoldingDto
    {
        public string CoinId { get; set; }             // CoinsMarket Id
        public string Symbol { get; set; }             // Coin symbol (BTC, ETH...)
        public decimal Quantity { get; set; }          // Quantity owned
        public decimal AverageBuyPrice { get; set; }   // Average buy price
        public decimal CurrentValue { get; set; }      // Current market value
        public decimal Profit { get; set; }            // Current profit/loss
        public DateTime LastUpdated { get; set; }      // Last updated holding
    }

}
