using CoinUpWorkerService.Schedulers;

namespace CoinUpWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Worker> _logger;

        public Worker(IServiceProvider serviceProvider, ILogger<Worker> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker démarré à {Time}", DateTimeOffset.Now);

            var interval = TimeSpan.FromMinutes(3); // Configure interval here

            try
            {
                using var scope = _serviceProvider.CreateScope();
                var scheduler = scope.ServiceProvider.GetRequiredService<JobScheduler>();

                // Call the scheduler once; it handles the periodic execution internally
                //await scheduler.ScheduleDataCollectionJob(interval, stoppingToken);
                await scheduler.ScheduleDataCollectionJobOnce();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la collecte des données");
            }

            _logger.LogWarning("⚠️ Worker arrêté suite à un signal d’annulation.");
        }
        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    _logger.LogInformation("Worker démarré à {Time}", DateTimeOffset.Now);

        //    var interval = TimeSpan.FromMinutes(1); // Configure interval here

        //    while (!stoppingToken.IsCancellationRequested)
        //    {
        //        _logger.LogInformation("⏳ Lancement de la collecte à {Time}", DateTimeOffset.Now);

        //        try
        //        {
        //            using var scope = _serviceProvider.CreateScope();
        //            var scheduler = scope.ServiceProvider.GetRequiredService<JobScheduler>();

        //            // Correct call: pass the interval and the cancellation token
        //            await scheduler.ScheduleDataCollectionJob(interval, stoppingToken);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "❌ Erreur lors de la collecte des données");
        //        }
        //    }

        //    _logger.LogWarning("⚠️ Worker arrêté suite à un signal d’annulation.");
        //}
    }
}
