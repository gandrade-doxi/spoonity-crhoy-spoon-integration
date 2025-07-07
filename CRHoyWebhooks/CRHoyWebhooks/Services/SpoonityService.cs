using CRHoyWebhooks.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

public interface ISpoonityService
{
    Task<bool> UserExistsByEmailAsync(string email);
    Task<SubscribedUser?> GetUserInfoByEmailAsync(string email);
}

public class SpoonityService : ISpoonityService
{
    private readonly HttpClient _httpClient;
    private readonly SpoonitySettings _settings;

    public SpoonityService(HttpClient httpClient, IOptions<SpoonitySettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public async Task<bool> UserExistsByEmailAsync(string email)
    {
        var body = new { card_number = email };
        var url1 = $"{_settings.Endpoint}/onscreen?api_key={_settings.ApiKey}";

        var response1 = await _httpClient.PostAsJsonAsync(url1, body);
        if (!response1.IsSuccessStatusCode) return false;

        var content1 = await response1.Content.ReadFromJsonAsync<OnscreenSessionResponse>();
        var hash = content1?.pos_session?.hash;
        if (string.IsNullOrEmpty(hash)) return false;

        var url2 = $"{_settings.Endpoint}/onscreen?api_key={_settings.ApiKey}&pos_session_hash={hash}";
        var response2 = await _httpClient.GetAsync(url2);
        if (!response2.IsSuccessStatusCode) return false;

        var content2 = await response2.Content.ReadFromJsonAsync<OnscreenUserResponse>();
        return !string.IsNullOrEmpty(content2?.user?.email_address);
    }

    public async Task<SubscribedUser?> GetUserInfoByEmailAsync(string email)
    {
        var body = new { card_number = email };
        var url1 = $"{_settings.Endpoint}/onscreen?api_key={_settings.ApiKey}";

        var response1 = await _httpClient.PostAsJsonAsync(url1, body);
        if (!response1.IsSuccessStatusCode) return null;

        var content1 = await response1.Content.ReadFromJsonAsync<OnscreenSessionResponse>();
        var hash = content1?.pos_session?.hash;
        if (string.IsNullOrEmpty(hash)) return null;

        var url2 = $"{_settings.Endpoint}/onscreen?api_key={_settings.ApiKey}&pos_session_hash={hash}";
        var response2 = await _httpClient.GetAsync(url2);
        if (!response2.IsSuccessStatusCode) return null;

        var content2 = await response2.Content.ReadFromJsonAsync<OnscreenUserResponse>();
        var u = content2?.user;
        if (u == null || string.IsNullOrEmpty(u.email_address)) return null;

        return new SubscribedUser
        {
            Email = u.email_address,
            FirstName = u.first_name,
            LastName = u.last_name,
            Cedula = u.cedula,
            PassportNumber = u.passport_number,
            PhoneNumber = u.phone_number,
            Address = u.address,
            CloverId = u.clover_id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}

public class OnscreenSessionResponse
{
    public PosSession? pos_session { get; set; }

    public class PosSession
    {
        public string? hash { get; set; }
    }
}

public class OnscreenUserResponse
{
    public User? user { get; set; }

    public class User
    {
        public string? email_address { get; set; }
        public string? first_name { get; set; }
        public string? last_name { get; set; }
        public string? cedula { get; set; }
        public string? passport_number { get; set; }
        public string? phone_number { get; set; }
        public string? address { get; set; }
        public string? clover_id { get; set; }
    }
}
