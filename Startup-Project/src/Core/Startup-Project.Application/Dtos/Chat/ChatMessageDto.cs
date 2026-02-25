namespace Startup_Project.Application.Dtos.Chat;

public class ChatMessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public string SenderFullName { get; set; } = null!;
    public string? SenderProfileImageUrl { get; set; }
    public string Content { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsMine { get; set; }
}

public class SendMessageDto
{
    public Guid ConversationId { get; set; }
    public string Content { get; set; } = null!;
}

public class PaginatedMessagesDto
{
    public List<ChatMessageDto> Messages { get; set; } = [];
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public bool HasMore { get; set; }
}

public class UnreadCountDto
{
    public int TotalUnread { get; set; }
}
