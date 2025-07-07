using Microsoft.AspNetCore.Mvc;

namespace CRHoyWebhooks.Controllers
{
    using CRHoyWebhooks.Models;
    using CRHoyWebhooks.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using System.Text.Json;

    [ApiController]
    [Route("api/webhook")]
    public class WebhookController : ControllerBase
    {
        private readonly ISpoonityService _spoonityService;
        private readonly ICrHoyService _crHoyService;
        private readonly AppDbContext _dbContext;

        public WebhookController(
            ISpoonityService spoonityService,
            ICrHoyService crHoyService,
            AppDbContext dbContext)
        {
            _spoonityService = spoonityService;
            _crHoyService = crHoyService;
            _dbContext = dbContext;
        }

        [HttpPost("subscription-created")]
        public async Task<IActionResult> HandleSubscriptionCreated([FromBody] WebhookEvent webhook)
        {
            if (webhook.EventType != "SUBSCRIPTION_CREATED")
                return BadRequest("Invalid event type");

            var email = webhook.Data?.Message;
            var userExists = await _spoonityService.UserExistsByEmailAsync(email);
            if (!userExists)
                return NotFound(new { message = "User not found in Spoonity" });

            var userInfo = await _spoonityService.GetUserInfoByEmailAsync(email);
            if (userInfo == null)
                return NotFound(new { message = "Failed to fetch user details" });

            var existing = await _dbContext.SubscribedUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (existing == null)
            {
                userInfo.SubscriptionDate = webhook.Timestamp;
                userInfo.IsActive = true;
                userInfo.LastEventType = webhook.EventType;
                _dbContext.SubscribedUsers.Add(userInfo);
            }
            else
            {
                existing.FirstName = userInfo.FirstName;
                existing.LastName = userInfo.LastName;
                existing.Cedula = userInfo.Cedula;
                existing.PassportNumber = userInfo.PassportNumber;
                existing.PhoneNumber = userInfo.PhoneNumber;
                existing.Address = userInfo.Address;
                existing.CloverId = userInfo.CloverId;
                existing.SubscriptionDate = webhook.Timestamp;
                existing.IsActive = true;
                existing.LastEventType = webhook.EventType;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "User subscribed and saved" });
        }

        [HttpPost("subscription-expired")]
        public async Task<IActionResult> HandleSubscriptionExpired([FromBody] WebhookEvent webhook)
        {
            if (webhook.EventType != "SUBSCRIPTION_EXPIRED")
                return BadRequest("Invalid event type");

            var email = webhook.Data?.Message;
            var user = await _dbContext.SubscribedUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
                return NotFound(new { message = "User not found in local DB" });

            user.IsActive = false;
            user.LastEventType = webhook.EventType;
            user.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "User marked as unsubscribed" });
        }
    }
}
