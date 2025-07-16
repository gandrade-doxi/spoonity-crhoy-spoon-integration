using CRHoyWebhooks.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRHoyWebhooks.Services;
using CRHoyWebhooks.Data;

[ApiController]
[Route("api/webhook")]
public class WebhookController : ControllerBase
{
    private readonly ISpoonityService _spoonityService;
    private readonly IEmailService _emailService;
    private readonly AppDbContext _dbContext;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<WebhookController> _logger;

    public WebhookController(
        ISpoonityService spoonityService,
        IEmailService emailService,
        AppDbContext dbContext,
        IWebHostEnvironment env,
        ILogger<WebhookController> logger)
    {
        _spoonityService = spoonityService;
        _emailService = emailService;
        _dbContext = dbContext;
        _env = env;
        _logger = logger;
    }

    [HttpPost("subscription-created")]
    public async Task<IActionResult> HandleSubscriptionCreated([FromBody] WebhookEvent webhook)
    {
        var email = webhook.User?.Email;
        var name = webhook.User?.Email ?? "Usuario";

        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Email is required" });

        var existsInSpoonity = await _spoonityService.UserExistsByEmailAsync(email);

        string templateFile = existsInSpoonity
            ? "WelcomeSpoonityUser.html"
            : "PromptRegisterSpoonity.html";

        string subject = existsInSpoonity
            ? "🎉 ¡Bienvenido a CRHoy PRO + Spooners! Ya podés disfrutar tus beneficios"
            : "🚀 Activá tus beneficios CRHoy PRO en Spooners";

        string htmlPath = Path.Combine(_env.WebRootPath, "Templates", templateFile);

        if (!System.IO.File.Exists(htmlPath))
        {
            _logger.LogError("📄 Template no encontrado en: {Path}", htmlPath);
            return StatusCode(500, new { message = $"Template no encontrado en: {htmlPath}" });
        }

        string htmlBody = await System.IO.File.ReadAllTextAsync(htmlPath);
        htmlBody = htmlBody.Replace("[Nombre]", name);

        var sent = await _emailService.SendEmailAsync(email, subject, htmlBody);

        if (!sent)
            return StatusCode(500, new { message = "No se pudo enviar el correo" });

        return Ok(new { message = "Correo enviado correctamente" });
    }

    [HttpPost("subscription-expired")]
    public async Task<IActionResult> HandleSubscriptionExpired([FromBody] WebhookEvent webhook)
    {
        var email = webhook.User?.Email;
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Email is required" });

        var existing = await _dbContext.SubscribedUsers.FirstOrDefaultAsync(u => u.Email == email);
        if (existing == null)
            return NotFound(new { message = "User not found in local DB" });

        existing.IsActive = false;
        existing.LastEventType = "SUBSCRIPTION_EXPIRED";
        existing.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return Ok(new { message = "User marked as unsubscribed" });
    }
}
