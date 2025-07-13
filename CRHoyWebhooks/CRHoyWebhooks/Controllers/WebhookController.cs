using CRHoyWebhooks.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CRHoyWebhooks.Services;
using CRHoyWebhooks.Models;
using CRHoyWebhooks.Data;

[ApiController]
[Route("api/webhook")]
public class WebhookController : ControllerBase
{
    private readonly ISpoonityService _spoonityService;
    private readonly IEmailService _emailService;
    private readonly AppDbContext _dbContext;

    public WebhookController(
        ISpoonityService spoonityService,
        IEmailService emailService,
        AppDbContext dbContext)
    {
        _spoonityService = spoonityService;
        _emailService = emailService;
        _dbContext = dbContext;
    }

    [HttpPost("subscription-created")]
    public async Task<IActionResult> HandleSubscriptionCreated([FromBody] WebhookEvent webhook)
    {
        var email = webhook.User?.Email;
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Email is required" });

        var existsInSpoonity = await _spoonityService.UserExistsByEmailAsync(email);

        var replacements = new Dictionary<string, string>
            {
                { "{{EMAIL}}", email },
                {
                    "{{BODY}}",
                    existsInSpoonity
                        ? "<p>¡Gracias por suscribirte! Ya formas parte del programa de beneficios de CRHoy.</p>"
                        : "<p>Gracias por suscribirte. Recuerda que debes registrarte en el programa de fidelidad en tu próxima visita.</p>"
                }
            };

        var sent = await _emailService.SendTemplatedEmailAsync(
            email,
            "¡Bienvenido a CRHoy Loyalty!",
            "SubscriptionCreatedEmail.html",
            replacements
        );

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
