using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.EntityFrameworkCore;
using Serilog;
using CRHoyWebhooks.Services;
using CRHoyWebhooks.Data;
using CRHoyWebhooks.Models;
using Microsoft.Extensions.Configuration;

Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .UseSerilog((context, services, config) =>
    {
        config.Enrich.FromLogContext()
              .WriteTo.File(
                  path: "Logs/rewards.log",
                  rollingInterval: RollingInterval.Day,
                  retainedFileCountLimit: 7,
                  outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss}] [{Level:u3}] {Message:lj}{NewLine}{Exception}");
    })
    .ConfigureServices((hostContext, services) =>
    {
        var configuration = hostContext.Configuration;

        // Bind appsettings sections
        services.Configure<SpoonitySettings>(configuration.GetSection("Spoonity"));

        // Database
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Services
        services.AddScoped<ISpoonityService, SpoonityService>();
        services.AddHttpClient();

        // Worker
        services.AddHostedService<Worker>();
    })
    .Build()
    .Run();
