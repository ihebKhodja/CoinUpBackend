using CoinUpWorkerService;
using CoinUpWorkerService.Data;
using CoinUpWorkerService.Jobs;
using CoinUpWorkerService.Schedulers;
using CoinUpWorkerService.Services;
using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHttpClient<IDataCollectorService, CoinCapService>();

builder.Services.AddScoped<DataCollectionJob>();
builder.Services.AddScoped<JobScheduler>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();


host.Run();
