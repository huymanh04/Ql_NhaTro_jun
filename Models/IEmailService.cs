namespace Ql_NhaTro_jun.Models
{
    public interface IEmailService
    {
    Task SendEmailAsync(string toEmail, string subject, string body);
    }
}
