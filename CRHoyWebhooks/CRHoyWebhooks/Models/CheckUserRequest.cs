namespace CRHoyWebhooks.Models
{
    public class CheckUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string SessionKey { get; set; } = string.Empty;
    }
}
