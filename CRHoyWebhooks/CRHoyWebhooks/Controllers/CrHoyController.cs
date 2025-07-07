namespace CRHoyWebhooks.Controllers
{
    using CRHoyWebhooks.Models;
    using CRHoyWebhooks.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    [ApiController]
    [Route("api/crhoy")]
    public class CrHoyController : ControllerBase
    {
        private readonly ICrHoyService _crHoyService;
        private readonly AppDbContext _dbContext;

        public CrHoyController(ICrHoyService crHoyService, AppDbContext dbContext)
        {
            _crHoyService = crHoyService;
            _dbContext = dbContext;
        }

        [HttpGet("check-user")]
        public async Task<IActionResult> CheckCrHoyUser([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return BadRequest(new { message = "Email is required" });

            var crHoyResult = await _crHoyService.VerifyUserAsync(email);
            if (crHoyResult == null || !crHoyResult.isVerified)
                return NotFound(new { message = "Por favor regístrate en CRHoy" });

            var existing = await _dbContext.SubscribedUsers.FirstOrDefaultAsync(u => u.Email == email);
            if (existing == null)
            {
                var newUser = new SubscribedUser
                {
                    Email = email,
                    FirstName = crHoyResult.userData?.firstName,
                    LastName = crHoyResult.userData?.lastName,
                    Cedula = crHoyResult.userData?.documentNumber,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IsActive = true,
                    LastEventType = "CRHOY_VERIFIED"
                };
                _dbContext.SubscribedUsers.Add(newUser);
            }
            else
            {
                existing.IsActive = true;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.LastEventType = "CRHOY_VERIFIED";
            }

            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                message = "Usuario verificado y registrado",
                user = crHoyResult.userData
            });
        }
    }
}
