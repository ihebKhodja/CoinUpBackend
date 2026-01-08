namespace CoinUpWorkerService.Jobs
{
    public class DataCollectionJobOptions
    {
        public int RateLimitMs { get; set; } = 10000;
        public bool ThrowOnError { get; set; } = false;

        /// <summary>
        /// Which market chart windows (in days) to fetch in ExecuteGetHistoryAllAsync.
        /// Keep this small to reduce external API calls (e.g. [1,7,30] or [1,7,30,365]).
        /// </summary>
        public int[] HistoryDaysOptions { get; set; } = new[] {  90 };

        /// <summary>
        /// Maximum time to keep retrying a given history window (e.g. 1d, 7d) in ExecuteGetHistoryAllAsync.
        /// Prevents infinite retries when an upstream dependency is down.
        /// </summary>
        public int HistoryWindowMaxRetryMinutes { get; set; } = 30;

        /// <summary>
        /// Optional cap on retry attempts per history window in ExecuteGetHistoryAllAsync.
        /// Set to 0 to disable.
        /// </summary>
        public int HistoryWindowMaxRetryAttempts { get; set; } = 0;

        /// <summary>
        /// Base delay (ms) between history window retry attempts in ExecuteGetHistoryAllAsync.
        /// Effective delay is max(HistoryWindowRetryDelayMs, RateLimitMs).
        /// </summary>
        public int HistoryWindowRetryDelayMs { get; set; } = 30_000;
    }
}
