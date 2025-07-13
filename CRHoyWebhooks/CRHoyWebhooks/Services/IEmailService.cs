namespace CRHoyWebhooks.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent);

        Task<bool> SendTemplatedEmailAsync(
            string toEmail,
            string subject,
            string templateName,
            Dictionary<string, string> replacements);
    }
}
