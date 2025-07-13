namespace CRHoyWebhooks.Models
{
    public class SubscribedUser
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Cedula { get; set; }
        public string? PassportNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? CloverId { get; set; }
        public DateTime? SubscriptionDate { get; set; }
        public bool IsActive { get; set; } = true;
        public string? LastEventType { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public string? SessionKey { get; set; }
        public DateTime? LastRewardedAt { get; set; }

    }
}
