using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.VisualBasic;
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
    public NotificationType Type { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public NotificationStatus Status { get; private set; }
    public DateTime? SentAt { get; private set; }
    public DateTime? ReadAt { get; private set; }
    private Notification() { }
    
    public void MarkAsSent()
    {
        this.Status = NotificationStatus.Sent;
        this.SentAt = DateTime.UtcNow;
    }

}
