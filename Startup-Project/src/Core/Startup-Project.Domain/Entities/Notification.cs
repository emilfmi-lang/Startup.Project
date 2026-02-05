using Startup_Project.Domain.Entities.Common;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums.Notifications;

namespace Startup_Project.Domain.Entities;

public class Notification : BaseEntity
{
    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;

    public string Title { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public string? ImageUrl { get; private set; }
    public string? ActionUrl { get; private set; }
    public string? Data { get; private set; }

    public NotificationType Type { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public NotificationStatus Status { get; private set; }

    public DateTime? SentAt { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public DateTime? ScheduledFor { get; private set; }

    public Guid? RelatedEntityId { get; private set; }
    public string? RelatedEntityType { get; private set; }

    private Notification() { }

    public Notification(
        Guid userId,
        string title,
        string body,
        NotificationType type,
        NotificationChannel channel,
        string? imageUrl = null,
        string? actionUrl = null,
        string? data = null,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null)
    {
        UserId = userId;
        Title = title;
        Body = body;
        Type = type;
        Channel = channel;
        ImageUrl = imageUrl;
        ActionUrl = actionUrl;
        Data = data;
        RelatedEntityId = relatedEntityId;
        RelatedEntityType = relatedEntityType;
        Status = NotificationStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public void MarkAsSent()
    {
        Status = NotificationStatus.Sent;
        SentAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDelivered()
    {
        Status = NotificationStatus.Delivered;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsRead()
    {
        if (Status == NotificationStatus.Read) return;

        Status = NotificationStatus.Read;
        ReadAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsFailed()
    {
        Status = NotificationStatus.Failed;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Schedule(DateTime scheduledFor)
    {
        Status = NotificationStatus.Scheduled;
        ScheduledFor = scheduledFor;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsRead => Status == NotificationStatus.Read;
    public bool IsSent => Status == NotificationStatus.Sent || Status == NotificationStatus.Delivered || Status == NotificationStatus.Read;
}
