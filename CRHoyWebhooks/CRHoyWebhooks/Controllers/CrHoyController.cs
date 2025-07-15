namespace CRHoyWebhooks.Controllers
{
    using CRHoyWebhooks.Models;
    using CRHoyWebhooks.Services;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
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

            if (existing != null)
            {
                var alreadyAwarded = await _dbContext.RewardLogs.AnyAsync(r => r.Email == request.Email && r.Source == "WEB");
                if (alreadyAwarded)
                {
                    return Conflict(new
                    {
                        error = "ALREADY_REWARDED",
                        message = "Ya reclamaste tus premios. Los próximos se harán de forma automática cada mes."
                    });
                }

                existing.IsActive = true;
                existing.UpdatedAt = DateTime.UtcNow;
                existing.LastEventType = "CRHOY_VERIFIED";
                existing.SessionKey = request.SessionKey;
            }
            else
            {
                existing = new SubscribedUser
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
                _dbContext.SubscribedUsers.Add(existing);
            }

            await _dbContext.SaveChangesAsync();

            var tokens = new[]
            {
                "132e92eca88c644426c2c456207bcbe5",
                "edc99da65ebf9149c2e011b769ea5ebb",
                "d89a6b18390605bd24a6e3a146709d4c",
                "c63b1d449ecd3acf28f1c8a7ad105dab"
            };

            var results = await _spoonityService.AwardPromotionTokensDetailedAsync(request.SessionKey, tokens);

            if (results.Any(r => !r.Success))
                return StatusCode(500, new { message = "Error al asignar uno o más premios" });

            foreach (var result in results)
            {
                _dbContext.RewardLogs.Add(new RewardLog
                {
                    Email = request.Email,
                    SessionKey = request.SessionKey,
                    Token = result.Token,
                    Success = result.Success,
                    Source = "WEB",
                    ResponseMessage = result.ResponseMessage,
                    Timestamp = DateTime.UtcNow
                });
            }


            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                message = "Usuario verificado, guardado y premios asignados exitosamente",
                user = crHoyResult.userData
            });
        }
    }
}
