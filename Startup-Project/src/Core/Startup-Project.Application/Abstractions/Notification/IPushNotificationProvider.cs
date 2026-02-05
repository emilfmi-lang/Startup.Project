namespace Startup_Project.Application.Abstractions.Notification;

public interface IPushNotificationProvider
{
    Task<bool> SendAsync(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string>? data = null);

    Task<int> SendToMultipleAsync(
        List<string> deviceTokens,
        string title,
        string body,
        Dictionary<string, string>? data = null);

    Task<bool> SendToTopicAsync(
        string topic,
        string title,
        string body,
        Dictionary<string, string>? data = null);
}
