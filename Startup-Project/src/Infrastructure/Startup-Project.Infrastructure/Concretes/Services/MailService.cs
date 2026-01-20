
using MailKit.Net.Smtp;
using MimeKit;
using Startup_Project.Application.Abstractions.Services;

namespace Startup_Project.Infrastructure.Concretes.Services;

public class MailService : IMailService
{
    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse("perviznovruzov136@gmail.com"));
        email.To.Add(MailboxAddress.Parse(toEmail));
        email.Subject = subject;
        email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };
        using var smtp = new SmtpClient();
        await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
        await smtp.AuthenticateAsync("perviznovruzov136@gmail.com", "qjpm nvkg lqsp wirf");
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}
