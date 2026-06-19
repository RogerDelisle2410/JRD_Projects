namespace JRD_Projects.Services
{
    using Microsoft.Extensions.Options;
    using System.Net;
    using System.Net.Mail;
    using Microsoft.AspNetCore.Http;
    using JRD_Projects.Models;

    public class EmailService
    {
        private readonly EmailSettings _settings;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public EmailService(
            IOptions<EmailSettings> settings,
            IHttpContextAccessor httpContextAccessor)
        {
            _settings = settings.Value;
            _httpContextAccessor = httpContextAccessor;
        }

        public void Send(string subject, string body)
        {
            var localIps = new[] { "127.0.0.1", "::1" };
            var remoteIp = _httpContextAccessor.HttpContext?
                .Connection?.RemoteIpAddress?.ToString();

            // Skip sending email when running locally
            if (localIps.Contains(remoteIp))
                return;

            using var client = new SmtpClient(_settings.Host)
            {
                Port = _settings.Port,
                EnableSsl = _settings.EnableSSL,
                Credentials = new NetworkCredential(_settings.From, _settings.Password)
            };

            using var mail = new MailMessage(_settings.From, _settings.From, subject, body);

            client.Send(mail);
        }
    }
} 