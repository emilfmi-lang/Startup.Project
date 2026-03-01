namespace Startup_Project.Application.Abstractions.AI;

public interface IAiProvider
{
    Task<AiProviderResponse> SendAsync(List<AiChatMessage> messages, CancellationToken ct = default);
}

public class AiChatMessage
{
    public string Role { get; set; } = null!;
    public string Content { get; set; } = null!;
}

public class AiProviderResponse
{
    public string Content { get; set; } = null!;
    public int TokensUsed { get; set; }
    public bool Success { get; set; }
    public string? Error { get; set; }
}
