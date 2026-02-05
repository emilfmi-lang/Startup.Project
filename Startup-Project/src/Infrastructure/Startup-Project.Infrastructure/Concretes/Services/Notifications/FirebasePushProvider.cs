using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Startup_Project.Application.Abstractions.Notification;

namespace Startup_Project.Infrastructure.Concretes.Services.Notifications;

public class FirebasePushProvider : IPushNotificationProvider
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<FirebasePushProvider> _logger;

    public FirebasePushProvider(
        IConfiguration configuration,
        ILogger<FirebasePushProvider> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendAsync(
        string deviceToken,
        string title,
        string body,
        Dictionary<string, string>? data = null)
    {
        try
        {
            // TODO: Firebase Admin SDK inteqrasiyası
            // var message = new Message
            // {
            //     Token = deviceToken,
            //     Notification = new FirebaseAdmin.Messaging.Notification
            //     //     Title = title,
            //     //     Body = body
            //     // },
            //     Data = data
            // };
            // await FirebaseMessaging.DefaultInstance.SendAsync(message);

            _logger.LogInformation(
                "Push notification sent to {Token}: {Title}",
                deviceToken[..Math.Min(20, deviceToken.Length)],
                title);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification to {Token}", deviceToken);
            return false;
        }
    }

    public async Task<int> SendToMultipleAsync(
        List<string> deviceTokens,
        string title,
        string body,
        Dictionary<string, string>? data = null)
    {
        var successCount = 0;

        foreach (var token in deviceTokens)
        {
            var success = await SendAsync(token, title, body, data);
            if (success) successCount++;
        }

        _logger.LogInformation(
            "Bulk push notification sent: {Success}/{Total}",
            successCount,
            deviceTokens.Count);

        return successCount;
    }

    public async Task<bool> SendToTopicAsync(
        string topic,
        string title,
        string body,
        Dictionary<string, string>? data = null)
    {
        try
        {
            // TODO: Firebase topic messaging
            // var message = new Message
            // {
            //     Topic = topic,
            //     Notification = new FirebaseAdmin.Messaging.Notification
            //     {
            //         Title = title,
            //         Body = body
            //     },
            //     Data = data
            // };
            // await FirebaseMessaging.DefaultInstance.SendAsync(message);

            _logger.LogInformation(
                "Topic push notification sent to {Topic}: {Title}",
                topic,
                title);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send topic push notification to {Topic}", topic);
            return false;
        }
    }
}
