using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.EntityFrameworkCore;
using Serilog;
using CRHoyWebhooks.Services;
using CRHoyWebhooks.Data;

try
{
    Host.CreateDefaultBuilder(args)
        .UseWindowsService()
        .ConfigureAppConfiguration((hostingContext, config) =>
        {
            // Asegura que appsettings.json se cargue correctamente
            config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        })
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
            var configuration = hostContext.Configuration
                ?? throw new InvalidOperationException("No se pudo cargar la configuración del Host");

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            services.AddScoped<ISpoonityService, SpoonityService>();
            services.AddHttpClient();

            services.AddHostedService<Worker>();
        })
        .Build()
        .Run();
}
catch (Exception ex)
{
    Console.WriteLine("❌ Error fatal al iniciar el Worker:");
    Console.WriteLine(ex.ToString());
    Log.Fatal(ex, "Error fatal al iniciar el Worker");
}
finally
{
    Log.CloseAndFlush();
}
