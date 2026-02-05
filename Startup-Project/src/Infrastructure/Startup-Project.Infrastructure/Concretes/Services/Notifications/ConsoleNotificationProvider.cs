using Startup_Project.Application.Abstractions.Notification;

namespace Startup_Project.Infrastructure.Concretes.Services.Notifications;

public class ConsoleNotificationProvider : IPushNotificationProvider
{
    public Task<bool> SendAsync(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string>? data = null)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine($"  📱 PUSH NOTIFICATION");
        Console.WriteLine($"  📍 Token: {deviceToken[..Math.Min(20, deviceToken.Length)]}...");
        Console.WriteLine($"  📋 Title: {title}");
        Console.WriteLine($"  📝 Body: {body}");
        if (data != null && data.Count > 0)
        {
            Console.WriteLine($"  📦 Data: {string.Join(", ", data.Select(x => $"{x.Key}={x.Value}"))}");
        }
        Console.WriteLine($"  ⏰ Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine("═══════════════════════════════════════════════════════");

        return Task.FromResult(true);
    }

    public Task<int> SendToMultipleAsync(
        List<string> deviceTokens,
        string title,
        string body,
        Dictionary<string, string>? data = null)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine($"  📱 BULK PUSH NOTIFICATION");
        Console.WriteLine($"  👥 Recipients: {deviceTokens.Count} devices");
        Console.WriteLine($"  📋 Title: {title}");
        Console.WriteLine($"  📝 Body: {body}");
        Console.WriteLine($"  ⏰ Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine("═══════════════════════════════════════════════════════");

        return Task.FromResult(deviceTokens.Count);
    }

    public Task<bool> SendToTopicAsync(
        string topic,
        string title,
        string body,
        Dictionary<string, string>? data = null)
    {
        Console.WriteLine("═══════════════════════════════════════════════════════");
        Console.WriteLine($"  📢 TOPIC PUSH NOTIFICATION");
        Console.WriteLine($"  🏷️ Topic: {topic}");
        Console.WriteLine($"  📋 Title: {title}");
        Console.WriteLine($"  📝 Body: {body}");
        Console.WriteLine($"  ⏰ Time: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        Console.WriteLine("═══════════════════════════════════════════════════════");

        return Task.FromResult(true);
    }
}
