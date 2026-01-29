namespace Startup_Project.Application.Abstractions.Services;

public interface ISmsService
{
    Task SendAsync(string phoneNumber, string message);
}
