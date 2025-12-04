namespace CoinUpAPI.Dto
{
    public class BuySellResponseDto
    {
        public string CoinId { get; set; }             // CoinsMarket Id
        public string Type { get; set; }               // "BUY" or "SELL"
        public decimal Quantity { get; set; }          // Amount bought/sold
        public decimal Price { get; set; }             // Price at operation
        public decimal NewBalance { get; set; }        // Wallet balance after operation
        public decimal CurrentValue { get; set; }      // Current value of this holding
        public decimal Profit { get; set; }            // Profit after transaction
    }

}
