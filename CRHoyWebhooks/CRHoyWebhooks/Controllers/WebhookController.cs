using CRHoyWebhooks.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/webhook")]
public class WebhookController : ControllerBase
{
    private readonly ISpoonityService _spoonityService;
    private readonly AppDbContext _dbContext;

    public WebhookController(
        ISpoonityService spoonityService,
        AppDbContext dbContext)
    {
        _spoonityService = spoonityService;
        _dbContext = dbContext;
    }

    [HttpPost("subscription-created")]
    public async Task<IActionResult> HandleSubscriptionCreated([FromBody] WebhookEvent webhook)
    {
        var email = webhook.User?.Email;
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { message = "Email is required" });

        var userExists = await _spoonityService.UserExistsByEmailAsync(email);
        if (!userExists)
            return NotFound(new { message = "User not found in Spoonity" });

        var userInfo = await _spoonityService.GetUserInfoByEmailAsync(email);
        if (userInfo == null)
            return NotFound(new { message = "Failed to fetch user details" });

        var existing = await _dbContext.SubscribedUsers.FirstOrDefaultAsync(u => u.Email == email);
        if (existing == null)
        {
            userInfo.SubscriptionDate = DateTime.UtcNow;
            userInfo.IsActive = true;
            userInfo.LastEventType = "SUBSCRIPTION_CREATED";
            userInfo.Cedula ??= webhook.User?.DocumentId;
            _dbContext.SubscribedUsers.Add(userInfo);
        }
        else
        {
            existing.FirstName = userInfo.FirstName;
            existing.LastName = userInfo.LastName;
            existing.Cedula = webhook.User?.DocumentId ?? existing.Cedula;
            existing.PassportNumber = userInfo.PassportNumber;
            existing.PhoneNumber = userInfo.PhoneNumber;
            existing.Address = userInfo.Address;
            existing.CloverId = userInfo.CloverId;
            existing.SubscriptionDate = DateTime.UtcNow;
            existing.IsActive = true;
            existing.LastEventType = "SUBSCRIPTION_CREATED";
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync();
        return Ok(new { message = "User subscribed and saved" });
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
