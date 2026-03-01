using Startup_Project.Application.Dtos.AI;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces;

public interface IAiChatService
{
    Task<ResponseModel<AiChatResponseDto>> SendMessageAsync(AiChatRequestDto request);
    Task<ResponseModel<List<AiConversationListDto>>> GetConversationsAsync();
    Task<ResponseModel<AiMessageListDto>> GetMessagesAsync(Guid conversationId, int page, int pageSize);
    Task<ResponseModel<bool>> DeleteConversationAsync(Guid conversationId);
}
