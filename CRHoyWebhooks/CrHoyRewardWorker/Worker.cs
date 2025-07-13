using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CRHoyWebhooks.Data;
using Microsoft.EntityFrameworkCore;
using CRHoyWebhooks.Models;
using CRHoyWebhooks.Services;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _services;

    public Worker(ILogger<Worker> logger, IServiceProvider services)
    {
        _logger = logger;
        _services = services;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("⏰ Servicio de premiación iniciado");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var spoonityService = scope.ServiceProvider.GetRequiredService<ISpoonityService>();

                var now = DateTime.UtcNow;

                var users = await db.SubscribedUsers
                    .Where(u => u.IsActive && u.SessionKey != null &&
                                (u.LastRewardedAt == null || EF.Functions.DateDiffDay(u.LastRewardedAt.Value, now) >= 30))
                    .ToListAsync(stoppingToken);

                var tokens = new[]
                {
                    "132e92eca88c644426c2c456207bcbe5", // 2x1 Capuccino 8oz
                    "edc99da65ebf9149c2e011b769ea5ebb", // 20% Lasagna
                    "d89a6b18390605bd24a6e3a146709d4c", // 20% Postres
                    "c63b1d449ecd3acf28f1c8a7ad105dab"  // 15% Chilenas
                };

                foreach (var user in users)
                {
                    _logger.LogInformation("🎯 Procesando usuario: {Email}", user.Email);

                    foreach (var token in tokens)
                    {
                        var success = await spoonityService.AwardPromotionTokensAsync(user.SessionKey!, new[] { token });

                        db.RewardLogs.Add(new RewardLog
                        {
                            Email = user.Email,
                            SessionKey = user.SessionKey,
                            Token = token,
                            Timestamp = now,
                            Success = success,
                            ResponseMessage = success ? "OK" : "Error al enviar token"
                        });

                        _logger.LogInformation("🎁 Token {Token} enviado a {Email} - Éxito: {Success}", token, user.Email, success);
                    }

                    user.LastRewardedAt = now;
                    await db.SaveChangesAsync(stoppingToken);
                }

                _logger.LogInformation("✅ Revisión completada: {Count} usuario(s) premiado(s)", users.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error durante el proceso de premiación");
            }

            // Esperar 24 horas
            await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
        }
    }
}
