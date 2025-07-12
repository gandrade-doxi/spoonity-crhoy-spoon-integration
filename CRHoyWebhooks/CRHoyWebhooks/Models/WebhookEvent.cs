namespace CRHoyWebhooks.Models
{
    public class WebhookEvent
    {
        public string WebhookId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? CurrentPeriodEnd { get; set; }
        public WebhookUser User { get; set; } = new();
    }

    public class WebhookUser
    {
        public string Email { get; set; } = string.Empty;
        public string DocumentId { get; set; } = string.Empty;
    }
}
