namespace JRD_Projects.Services
{
    using Microsoft.Extensions.Options;
    using System.Net;
    using System.Net.Mail;

    public class EmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public void Send(string subject, string body)
        {
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
