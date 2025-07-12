namespace CRHoyWebhooks.Controllers
{
    using CRHoyWebhooks.Models;
    using CRHoyWebhooks.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using CRHoyWebhooks.Services;

    [ApiController]
    [Route("api/crhoy")]
    public class CrHoyController : ControllerBase
    {
        private readonly ICrHoyService _crHoyService;
        private readonly ISpoonityService _spoonityService;
        private readonly AppDbContext _dbContext;

        public CrHoyController(
            ICrHoyService crHoyService,
            ISpoonityService spoonityService,
            AppDbContext dbContext)
        {
            _crHoyService = crHoyService;
            _spoonityService = spoonityService;
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

            // Obtener session_key desde Spoonity
            var sessionKey = await _spoonityService.GetSessionKeyByEmailAsync(email);
            if (string.IsNullOrEmpty(sessionKey))
                return StatusCode(500, new { message = "Error al obtener session_key de Spoonity" });

            var tokens = new[]
            {
        "132e92eca88c644426c2c456207bcbe5", // Capuccino 2x1
        "edc99da65ebf9149c2e011b769ea5ebb", // Lasagna 20%
        "d89a6b18390605bd24a6e3a146709d4c", // Postres 20%
        "c63b1d449ecd3acf28f1c8a7ad105dab"  // Chilenas 15%
    };

            var awarded = await _spoonityService.AwardPromotionTokensAsync(sessionKey, tokens);
            if (!awarded)
                return StatusCode(500, new { message = "Error al asignar los premios" });

            return Ok(new
            {
                message = "Usuario verificado, guardado y premios asignados exitosamente",
                user = crHoyResult.userData
            });
        }

    }
}
