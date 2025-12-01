using CoinUpWorkerService.Jobs;

namespace CoinUpWorkerService.Schedulers
{
    public class JobScheduler
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<JobScheduler> _logger;

        public JobScheduler(IServiceProvider serviceProvider, ILogger<JobScheduler> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        /// <summary>
        /// Execute the job once.
        /// This is called by the Worker.
        /// </summary>
        public async Task ScheduleDataCollectionJobOnce()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var job = scope.ServiceProvider.GetRequiredService<DataCollectionJob>();

                _logger.LogInformation("➡️ Exécution du DataCollectionJob...");
                //await job.ExecuteGetMarketAsync();
                await job.ExecuteGetHistoryAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de l’exécution du job");
            }
        }

        /// <summary>
        /// Runs indefinitely every interval, if you want a stand-alone scheduler.
        /// (Not used by Worker but kept clean & functional)
        /// </summary>
        public async Task ScheduleDataCollectionJob(TimeSpan interval, CancellationToken token)
        {
            _logger.LogInformation("Scheduler démarré. Intervalle : {Interval}", interval);

            var timer = new PeriodicTimer(interval);

            while (await timer.WaitForNextTickAsync(token) && !token.IsCancellationRequested)
            {
                await ScheduleDataCollectionJobOnce();
            }
        }
    }
}
