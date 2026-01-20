

namespace Startup_Project.Application.Abstractions.Services;

public interface IMailService
{
    Task SendEmailAsync(string toEmail, string subject, string body);
}
