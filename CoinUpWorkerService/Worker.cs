using CoinUpWorkerService.Schedulers;

namespace CoinUpWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        public Worker(IServiceProvider serviceProvider, ILogger<Worker> logger, IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Worker démarré à {Time}", DateTimeOffset.Now);

            try
            {
                var marketMinutes = _configuration.GetValue<int?>("Scheduler:MarketIntervalMinutes") ?? 1440;
                var historyMinutes = _configuration.GetValue<int?>("Scheduler:HistoryIntervalMinutes") ?? 60;

                if (marketMinutes <= 0) marketMinutes = 1440;
                if (historyMinutes <= 0) historyMinutes = 60;

                using var scope = _serviceProvider.CreateScope();
                var scheduler = scope.ServiceProvider.GetRequiredService<JobScheduler>();
                await scheduler.ScheduleDataCollectionJobs(
                    TimeSpan.FromMinutes(marketMinutes),
                    TimeSpan.FromMinutes(historyMinutes),
                    stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Erreur lors de la collecte des données");
            }

            _logger.LogWarning("⚠️ Worker arrêté suite à un signal d’annulation.");
        }

    }
}
