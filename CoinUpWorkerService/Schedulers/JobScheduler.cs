using CoinUpWorkerService.Jobs;

namespace CoinUpWorkerService.Schedulers
{
    public class JobScheduler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobScheduler> _logger;

        private readonly SemaphoreSlim _marketLock = new(1, 1);
        private readonly SemaphoreSlim _historyLock = new(1, 1);

        public JobScheduler(IServiceProvider serviceProvider, ILogger<JobScheduler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Execute the job once.
        /// This is called by the Worker.
        /// </summary>
        public async Task ScheduleMarketJobOnce()
        {
            if (!await _marketLock.WaitAsync(0))
            {
                _logger.LogWarning("Market job skipped: previous execution still running.");
                return;
            }

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<DataCollectionJob>();

                _logger.LogInformation("➡️ Exécution du job Market...");
                await job.ExecuteGetMarketAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de l’exécution du job Market");
            }
            finally
            {
                _marketLock.Release();
            }
        }

        public async Task ScheduleHistoryAllJobOnce()
        {
            if (!await _historyLock.WaitAsync(0))
            {
                _logger.LogWarning("History job skipped: previous execution still running.");
                return;
            }

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<DataCollectionJob>();

                _logger.LogInformation("➡️ Exécution du job History (all windows)...");
                await job.ExecuteGetHistoryAllAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de l’exécution du job History");
            }
            finally
            {
                _historyLock.Release();
            }
        }

        /// <summary>
        /// Runs indefinitely every interval, if you want a stand-alone scheduler.
        /// (Not used by Worker but kept clean & functional)
        /// </summary>
        public async Task ScheduleDataCollectionJobs(TimeSpan marketInterval, TimeSpan historyInterval, CancellationToken token)
        {
            _logger.LogInformation(
                "Schedulers démarrés. Market: {MarketInterval} | History: {HistoryInterval}",
                marketInterval,
                historyInterval);

            var marketTask = RunPeriodic(marketInterval, token, ScheduleMarketJobOnce);
            var historyTask = RunPeriodic(historyInterval, token, ScheduleHistoryAllJobOnce);

            await Task.WhenAll(marketTask, historyTask);
        }

        private async Task RunPeriodic(TimeSpan interval, CancellationToken token, Func<Task> action)
        {
            var timer = new PeriodicTimer(interval);
            try
            {
                // Run once immediately on startup
                if (!token.IsCancellationRequested)
                {
                    await action();
                }

                while (await timer.WaitForNextTickAsync(token) && !token.IsCancellationRequested)
                {
                    await action();
                }
            }
            catch (OperationCanceledException)
            {
                // Normal shutdown
            }
            finally
            {
                timer.Dispose();
            }
        }
    }
}
