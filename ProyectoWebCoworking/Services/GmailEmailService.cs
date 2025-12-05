using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace ProyectoWebCoworking.Services
{
    public class GmailEmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public GmailEmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string emailDestino, string asunto, string mensaje)
        {
            string emailOrigen = _configuration["EmailSettings:EmailOrigen"];
            string password = _configuration["EmailSettings:PasswordAplicacion"];
            string host = _configuration["EmailSettings:Host"];
            int port = int.Parse(_configuration["EmailSettings:Port"]);

            using (var smtpClient = new SmtpClient(host))
            {
                smtpClient.Port = port;
                smtpClient.Credentials = new NetworkCredential(emailOrigen, password);
                smtpClient.EnableSsl = true;
                
                using (var mailMensaje = new MailMessage())
                {
                    mailMensaje.From = new MailAddress(emailOrigen, "Coworking App");
                    mailMensaje.Subject = asunto;
                    mailMensaje.Body = mensaje;
                    mailMensaje.IsBodyHtml = false;
                    mailMensaje.To.Add(emailDestino);

                    await smtpClient.SendMailAsync(mailMensaje);
                }
            }
        }
    }
}
