using Startup_Project.Application.Dtos.Notification;
using Startup_Project.Application.Models;
using Startup_Project.Domain.Enums.Notifications;

namespace Startup_Project.Application.Abstractions.Notification;

public interface INotificationService
{
    // User Endpoints
    Task<ResponseModel<NotificationListDto>> GetMyNotificationsAsync(int page, int pageSize);
    Task<ResponseModel<int>> GetUnreadCountAsync();
    Task<ResponseModel<bool>> MarkAsReadAsync(Guid notificationId);
    Task<ResponseModel<bool>> MarkAllAsReadAsync();
    Task<ResponseModel<bool>> DeleteAsync(Guid notificationId);

    // Device Management
    Task<ResponseModel<bool>> RegisterDeviceAsync(RegisterDeviceDto dto);
    Task<ResponseModel<bool>> UnregisterDeviceAsync(string deviceToken);

    // Internal (digər service-lər çağırır)
    Task SendAsync(
        Guid userId,
        NotificationType type,
        string title,
        string body,
        NotificationChannel channel = NotificationChannel.Push,
        string? imageUrl = null,
        string? actionUrl = null,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null);

    Task SendBulkAsync(
        List<Guid> userIds,
        NotificationType type,
        string title,
        string body,
        NotificationChannel channel = NotificationChannel.Push);

    // Admin
    Task<ResponseModel<Guid>> AdminSendAsync(SendNotificationDto dto);
    Task<ResponseModel<bool>> AdminBroadcastAsync(string title, string body, NotificationType type);
}
