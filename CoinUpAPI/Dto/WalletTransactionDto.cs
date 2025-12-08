namespace CoinUpAPI.Dto
{
    public class WalletTransactionDto
    {
        public string Id { get; set; }      // WalletTransaction Id
        public string CoinId { get; set; }             // CoinsMarket Id
        public string Type { get; set; }               // "BUY" or "SELL"
        public decimal Quantity { get; set; }          // Quantity bought/sold
        public decimal PriceAtOperation { get; set; }  // Price at transaction
        public DateTime Timestamp { get; set; }        // Transaction time
    }

}
