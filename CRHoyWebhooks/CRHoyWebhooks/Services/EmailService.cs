namespace CRHoyWebhooks.Services
{
    using Microsoft.Extensions.Options;
    using SendGrid;
    using SendGrid.Helpers.Mail;
    using global::CRHoyWebhooks.Models;
    using System.Net.Mail;

    namespace CRHoyWebhooks.Services
    {
        public class EmailService : IEmailService
        {
            private readonly string _apiKey;
            private readonly string _fromEmail;
            private readonly string _fromName;

            public EmailService(IOptions<SendGridSettings> settings)
            {
                var config = settings.Value;
                _apiKey = config.ApiKey;
                _fromEmail = config.FromEmail;
                _fromName = config.FromName;
            }

            public async Task<bool> SendEmailAsync(string toEmail, string subject, string htmlContent)
            {
                var client = new SendGridClient(_apiKey);
                var from = new EmailAddress(_fromEmail, _fromName);
                var to = new EmailAddress(toEmail);
                var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent);
                var response = await client.SendEmailAsync(msg);
                return response.IsSuccessStatusCode;
            }

            public async Task<bool> SendTemplatedEmailAsync(string toEmail, string subject, string templateName, Dictionary<string, string> replacements)
            {
                var htmlContent = LoadEmailTemplate(templateName, replacements);
                return await SendEmailAsync(toEmail, subject, htmlContent);
            }

            private string LoadEmailTemplate(string fileName, Dictionary<string, string> replacements)
            {
                var path = Path.Combine(Directory.GetCurrentDirectory(), "Templates", fileName);
                var html = File.ReadAllText(path);

                foreach (var kvp in replacements)
                {
                    html = html.Replace(kvp.Key, kvp.Value);
                }

                return html;
            }
        }
    }

}
