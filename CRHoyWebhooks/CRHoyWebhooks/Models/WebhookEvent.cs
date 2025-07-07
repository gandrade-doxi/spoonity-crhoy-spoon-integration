namespace CRHoyWebhooks.Models
{
    public class WebhookEvent
    {
        public string EventType { get; set; }
        public DateTime Timestamp { get; set; }
        public WebhookData Data { get; set; }
    }

    public class WebhookData
    {
        public bool TestEvent { get; set; }
        public string WebhookId { get; set; }
        public string Message { get; set; }
    }
}
