namespace CoinUpAPI.Dto
{
    public class EWalletDto
    {
        public string WalletId { get; set; }           // Wallet ID
        public decimal Balance { get; set; }           // Current balance
        public List<CoinHoldingDto> Holdings { get; set; } = new List<CoinHoldingDto>();

    }
}
