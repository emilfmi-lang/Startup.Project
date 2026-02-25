namespace Startup_Project.Application.Dtos.Chat;

public class ConversationListDto
{
    public Guid Id { get; set; }
    public Guid OtherUserId { get; set; }
    public string OtherUserFullName { get; set; } = null!;
    public string? OtherUserProfileImageUrl { get; set; }
    public bool IsOtherUserOnline { get; set; }
    public string? LastMessagePreview { get; set; }
    public DateTime? LastMessageAt { get; set; }
    public int UnreadCount { get; set; }
}

public class CreateConversationDto
{
    public Guid OtherUserId { get; set; }
}

public class ConversationCreatedDto
{
    public Guid ConversationId { get; set; }
    public bool AlreadyExisted { get; set; }
}

public class ChatUserDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = null!;
    public string? ProfileImageUrl { get; set; }
    public bool IsOnline { get; set; }
}
