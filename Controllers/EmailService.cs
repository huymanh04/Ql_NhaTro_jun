using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using Ql_NhaTro_jun.Models;
using System.Net.Mail;
using System.Net;
using SmtpClient = System.Net.Mail.SmtpClient;
namespace Ql_NhaTro_jun.Controllers
{
    public class EmailService : IEmailService
    {
        private readonly SmtpSettings _smtp;

        public EmailService(IOptions<SmtpSettings> smtpOptions)
        {
            _smtp = smtpOptions.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(_smtp.UserName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            using var smtpClient = new SmtpClient(_smtp.Host, _smtp.Port)
            {
                Credentials = new NetworkCredential(_smtp.UserName, _smtp.Password),
                EnableSsl = _smtp.EnableSsl
            };

            await smtpClient.SendMailAsync(mail);
        }
    }

}
