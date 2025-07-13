namespace CRHoyWebhooks.Controllers
{
    using CRHoyWebhooks.Models;
    using CRHoyWebhooks.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using CRHoyWebhooks.Services;
    using CRHoyWebhooks.Data;

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

        [HttpPost("check-user")]
        public async Task<IActionResult> CheckCrHoyUser([FromBody] CheckUserRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.SessionKey))
                return BadRequest(new { message = "Email y sessionKey son requeridos" });

            var crHoyResult = await _crHoyService.VerifyUserAsync(request.Email);
            if (crHoyResult == null || !crHoyResult.isVerified)
            {
                return StatusCode(403, new
                {
                    error = "NOT_CRHOY_USER",
                    message = "El usuario no pertenece a CRHoy. Te invitamos a registrarte para acceder a los beneficios."
                });
            }

            var existing = await _dbContext.SubscribedUsers.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (existing == null)
            {
                var newUser = new SubscribedUser
                {
                    Email = request.Email,
                    FirstName = crHoyResult.userData?.firstName,
                    LastName = crHoyResult.userData?.lastName,
                    Cedula = crHoyResult.userData?.documentNumber,
                    SessionKey = request.SessionKey,
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

            var tokens = new[]
            {
        "132e92eca88c644426c2c456207bcbe5", // Capuccino 2x1
        "edc99da65ebf9149c2e011b769ea5ebb", // Lasagna 20%
        "d89a6b18390605bd24a6e3a146709d4c", // Postres 20%
        "c63b1d449ecd3acf28f1c8a7ad105dab"  // Chilenas 15%
    };

            var awarded = await _spoonityService.AwardPromotionTokensAsync(request.SessionKey, tokens);
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
