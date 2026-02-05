namespace Startup_Project.Application.Dtos.Notification;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Body { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public string? ActionUrl { get; set; }
    public string Type { get; set; } = null!;
    public string Channel { get; set; } = null!;
    public string Status { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SentAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? RelatedEntityType { get; set; }
}
