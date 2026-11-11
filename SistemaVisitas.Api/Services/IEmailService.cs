using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace SistemaVisitas.Api.Services
{
    public interface IEmailService
    {
        Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml, byte[]? adjuntoPdf = null, string? nombreAdjunto = null);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml, byte[]? adjuntoPdf = null, string? nombreAdjunto = null)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(
                _config["SmtpSettings:SenderName"],
                _config["SmtpSettings:SenderEmail"]));
            email.To.Add(new MailboxAddress("", destinatario));
            email.Subject = asunto;

            var builder = new BodyBuilder { HtmlBody = cuerpoHtml };

            // Si le pasamos un PDF generado, lo adjunta automáticamente
            if (adjuntoPdf != null && !string.IsNullOrEmpty(nombreAdjunto))
            {
                builder.Attachments.Add(nombreAdjunto, adjuntoPdf, new ContentType("application", "pdf"));
            }

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            try
            {
                await smtp.ConnectAsync(_config["SmtpSettings:Server"], int.Parse(_config["SmtpSettings:Port"]!), SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_config["SmtpSettings:Username"], _config["SmtpSettings:Password"]);
                await smtp.SendAsync(email);
            }
            finally
            {
                await smtp.DisconnectAsync(true);
            }
        }
    }
}