using Startup_Project.Domain.Enums.Notifications;

namespace Startup_Project.Application.Dtos.Notification;

public class SendNotificationDto
{
    public Guid? UserId { get; set; }
    public List<Guid>? UserIds { get; set; }
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public string? ActionUrl { get; set; }
    public NotificationType Type { get; set; }
    public NotificationChannel Channel { get; set; } = NotificationChannel.Push;
    public DateTime? ScheduledFor { get; set; }
}
