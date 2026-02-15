
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Startup_Project.Application.Abstractions.Services;
using Startup_Project.Infrastructure.Security;

namespace Startup_Project.Infrastructure.Concretes.Services;

public class MailService(
    IOptions<EmailSettings> emailOptions,
    ILogger<MailService> logger) : IMailService
{
    private readonly EmailSettings _settings = emailOptions.Value;

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.SenderName, _settings.SenderEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

        using var smtp = new SmtpClient();

        try
        {
            // Gmail SMTP — SecureSocketOptions.StartTls istifade olunmalidir
            await smtp.ConnectAsync(_settings.SmtpServer, _settings.Port, SecureSocketOptions.StartTls);

            // Google App Password istifade olunur (normal Gmail parolu deyil)
            await smtp.AuthenticateAsync(_settings.SenderEmail, _settings.AppPassword);

            await smtp.SendAsync(message);

            logger.LogInformation("Email ugurla gonderildi: {ToEmail}, Subject: {Subject}", toEmail, subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Email gonderme xetasi: {ToEmail}, Subject: {Subject}", toEmail, subject);
            throw new InvalidOperationException("Email göndərilərkən xəta baş verdi.", ex);
        }
        finally
        {
            if (smtp.IsConnected)
                await smtp.DisconnectAsync(true);
        }
    }
}
