using CRHoyWebhooks.Models;
using Microsoft.Extensions.Options;

namespace CRHoyWebhooks.Services
{
    public interface ICrHoyService
    {
        Task<CrHoyVerificationResult?> VerifyUserAsync(string email);
    }

    public class CrHoyService : ICrHoyService
    {
        private readonly HttpClient _httpClient;
        private readonly CrHoySettings _settings;

        public CrHoyService(HttpClient httpClient, IOptions<CrHoySettings> settings)
        {
            _httpClient = httpClient;
            _settings = settings.Value;
        }

        public async Task<CrHoyVerificationResult?> VerifyUserAsync(string email)
        {
            var tokenResponse = await _httpClient.PostAsJsonAsync(_settings.AuthUrl, new { apiKey = _settings.ApiKey });
            if (!tokenResponse.IsSuccessStatusCode) return null;

            var tokenData = await tokenResponse.Content.ReadFromJsonAsync<CrHoyTokenResponse>();
            if (tokenData?.token == null) return null;

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_settings.VerifyUrl}?email={email}");
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenData.token);

            var verifyResponse = await _httpClient.SendAsync(request);
            if (!verifyResponse.IsSuccessStatusCode) return null;

            return await verifyResponse.Content.ReadFromJsonAsync<CrHoyVerificationResult>();
        }
    }

    // DTOs
    public class CrHoyTokenResponse
    {
        public string token { get; set; }
    }

    public class CrHoyVerificationResult
    {
        public string identifier { get; set; }
        public bool isVerified { get; set; }
        public CrHoyUserData? userData { get; set; }
    }

    public class CrHoyUserData
    {
        public string email { get; set; }
        public string documentNumber { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
    }
}
