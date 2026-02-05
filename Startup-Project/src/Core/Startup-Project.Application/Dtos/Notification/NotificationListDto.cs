namespace Startup_Project.Application.Dtos.Notification;

public class NotificationListDto
{
    public List<NotificationDto> Notifications { get; set; } = [];
    public int TotalCount { get; set; }
    public int UnreadCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
