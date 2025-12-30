namespace CoinUpWorkerService.Jobs
{
    public class DataCollectionJobOptions
    {
        public int RateLimitMs { get; set; } = 10000;
        public bool ThrowOnError { get; set; } = false;
    }
}
