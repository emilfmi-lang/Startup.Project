using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Abstractions.Chat;
using Startup_Project.Application.Dtos.Chat;
using Startup_Project.Application.Interfaces;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums.Errors;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class ChatService : IChatService
{
    private readonly IApplicationDbContext _context;
    private readonly UserManager<AppUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IPresenceTracker _presenceTracker;

    public ChatService(
        IApplicationDbContext context,
        UserManager<AppUser> userManager,
        IHttpContextAccessor httpContextAccessor,
        IPresenceTracker presenceTracker)
    {
        _context = context;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
        _presenceTracker = presenceTracker;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            throw new BusinessException(BusinessErrorCodes.UnauthorizedAccess, "?stifad?çi t?yin edil? bilm?di.");
        return userId;
    }

    public async Task<ResponseModel<List<ConversationListDto>>> GetConversationsAsync()
    {
        var currentUserId = GetCurrentUserId();

        var participations = await _context.ConversationParticipants
            .Where(cp => cp.UserId == currentUserId && !cp.IsDeleted && !cp.IsArchived)
            .Include(cp => cp.Conversation)
            .ThenInclude(c => c.Participants)
            .ThenInclude(p => p.User)
            .OrderByDescending(cp => cp.Conversation.LastMessageAt ?? cp.Conversation.CreatedAt)
            .ToListAsync();

        var result = new List<ConversationListDto>();

        foreach (var cp in participations)
        {
            var otherParticipant = cp.Conversation.Participants
                .FirstOrDefault(p => p.UserId != currentUserId);

            if (otherParticipant == null) continue;

            var otherUser = otherParticipant.User;
            var isOnline = await _presenceTracker.IsUserOnline(otherUser.Id);

            result.Add(new ConversationListDto
            {
                Id = cp.ConversationId,
                OtherUserId = otherUser.Id,
                OtherUserFullName = otherUser.FullName,
                OtherUserProfileImageUrl = otherUser.ProfileImageUrl,
                IsOtherUserOnline = isOnline,
                LastMessagePreview = cp.Conversation.LastMessagePreview,
                LastMessageAt = cp.Conversation.LastMessageAt,
                UnreadCount = cp.UnreadCount
            });
        }

        return ResponseModel<List<ConversationListDto>>.SuccessResponse(result);
    }

    public async Task<ResponseModel<ConversationCreatedDto>> GetOrCreateConversationAsync(CreateConversationDto dto)
    {
        var currentUserId = GetCurrentUserId();

        if (dto.OtherUserId == currentUserId)
            throw new BusinessException(BusinessErrorCodes.ChatCannotMessageSelf, "Özünüz? mesaj gönd?r? bilm?zsiniz.");

        var otherUser = await _userManager.FindByIdAsync(dto.OtherUserId.ToString());
        if (otherUser == null)
            throw new NotFoundException(BusinessErrorCodes.ChatUserNotFound, $"?stifad?çi tap?lmad?. ID: {dto.OtherUserId}");

        // Mövcud conversation axtar???
        var existingConversation = await _context.ConversationParticipants
            .Where(cp => cp.UserId == currentUserId && !cp.Conversation.IsDeleted)
            .Where(cp => cp.Conversation.Participants.Any(p => p.UserId == dto.OtherUserId))
            .Select(cp => new { cp.ConversationId, cp.IsArchived, cp.Id })
            .FirstOrDefaultAsync();

        if (existingConversation != null)
        {
            // Arxivl?nmi?dirs? unarchive et
            if (existingConversation.IsArchived)
            {
                var participant = await _context.ConversationParticipants
                    .FirstAsync(cp => cp.Id == existingConversation.Id);
                participant.Unarchive();
                await _context.SaveChangesAsync();
            }

            return ResponseModel<ConversationCreatedDto>.SuccessResponse(new ConversationCreatedDto
            {
                ConversationId = existingConversation.ConversationId,
                AlreadyExisted = true
            });
        }

        // Yeni conversation yarat
        var conversation = new Conversation(Guid.NewGuid());
        await _context.Conversations.AddAsync(conversation);

        var participant1 = new ConversationParticipant(conversation.Id, currentUserId);
        var participant2 = new ConversationParticipant(conversation.Id, dto.OtherUserId);

        await _context.ConversationParticipants.AddAsync(participant1);
        await _context.ConversationParticipants.AddAsync(participant2);

        await _context.SaveChangesAsync();

        return ResponseModel<ConversationCreatedDto>.SuccessResponse(new ConversationCreatedDto
        {
            ConversationId = conversation.Id,
            AlreadyExisted = false
        });
    }

    public async Task<ResponseModel<PaginatedMessagesDto>> GetMessagesAsync(Guid conversationId, int page = 1, int pageSize = 30)
    {
        var currentUserId = GetCurrentUserId();

        var isParticipant = await _context.ConversationParticipants
            .AnyAsync(cp => cp.ConversationId == conversationId && cp.UserId == currentUserId && !cp.IsDeleted);

        if (!isParticipant)
            throw new BusinessException(BusinessErrorCodes.ChatNotParticipant, "Bu söhb?t? giri? icaz?niz yoxdur.");

        var totalCount = await _context.ChatMessages
            .CountAsync(m => m.ConversationId == conversationId && !m.IsDeleted);

        var messages = await _context.ChatMessages
            .Where(m => m.ConversationId == conversationId && !m.IsDeleted)
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.Sender)
            .ToListAsync();

        var messageDtos = messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new ChatMessageDto
            {
                Id = m.Id,
                SenderId = m.SenderId,
                SenderFullName = m.Sender.FullName,
                SenderProfileImageUrl = m.Sender.ProfileImageUrl,
                Content = m.Content,
                IsRead = m.IsRead,
                ReadAt = m.ReadAt,
                CreatedAt = m.CreatedAt,
                IsMine = m.SenderId == currentUserId
            })
            .ToList();

        return ResponseModel<PaginatedMessagesDto>.SuccessResponse(new PaginatedMessagesDto
        {
            Messages = messageDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            HasMore = (page * pageSize) < totalCount
        });
    }

    public async Task<ChatMessageDto> SendMessageAsync(SendMessageDto dto)
    {
        var currentUserId = GetCurrentUserId();

        var conversation = await _context.Conversations
            .Include(c => c.Participants)
            .FirstOrDefaultAsync(c => c.Id == dto.ConversationId && !c.IsDeleted);

        if (conversation == null)
            throw new NotFoundException(BusinessErrorCodes.ChatConversationNotFound, "Söhb?t tap?lmad?.");

        var isParticipant = conversation.Participants.Any(p => p.UserId == currentUserId && !p.IsDeleted);
        if (!isParticipant)
            throw new BusinessException(BusinessErrorCodes.ChatNotParticipant, "Bu söhb?t? giri? icaz?niz yoxdur.");

        var message = new ChatMessage(conversation.Id, currentUserId, dto.Content);
        await _context.ChatMessages.AddAsync(message);

        conversation.UpdateLastMessage(dto.Content);

        // Dig?r participant-?n unread count-unu art?r
        var otherParticipants = conversation.Participants
            .Where(p => p.UserId != currentUserId && !p.IsDeleted)
            .ToList();

        foreach (var participant in otherParticipants)
        {
            participant.IncrementUnread();

            // Arxivl?nmi?dirs? unarchive et (yeni mesaj g?ldi)
            if (participant.IsArchived)
                participant.Unarchive();
        }

        await _context.SaveChangesAsync();

        var sender = await _userManager.FindByIdAsync(currentUserId.ToString());

        return new ChatMessageDto
        {
            Id = message.Id,
            SenderId = message.SenderId,
            SenderFullName = sender!.FullName,
            SenderProfileImageUrl = sender.ProfileImageUrl,
            Content = message.Content,
            IsRead = message.IsRead,
            ReadAt = message.ReadAt,
            CreatedAt = message.CreatedAt,
            IsMine = true
        };
    }

    public async Task<ResponseModel<bool>> MarkConversationAsReadAsync(Guid conversationId)
    {
        var currentUserId = GetCurrentUserId();

        var participant = await _context.ConversationParticipants
            .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId
                && cp.UserId == currentUserId
                && !cp.IsDeleted);

        if (participant == null)
            throw new BusinessException(BusinessErrorCodes.ChatNotParticipant, "Bu söhb?t? giri? icaz?niz yoxdur.");

        participant.MarkAsRead();

        // Bu conversation-dak? qar?? t?r?fin gönd?rdiyi oxunmam?? mesajlar? i?ar?l?
        var unreadMessages = await _context.ChatMessages
            .Where(m => m.ConversationId == conversationId
                && m.SenderId != currentUserId
                && !m.IsRead
                && !m.IsDeleted)
            .ToListAsync();

        foreach (var message in unreadMessages)
        {
            message.MarkAsRead();
        }

        await _context.SaveChangesAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<UnreadCountDto>> GetUnreadCountAsync()
    {
        var currentUserId = GetCurrentUserId();

        var totalUnread = await _context.ConversationParticipants
            .Where(cp => cp.UserId == currentUserId && !cp.IsDeleted && !cp.IsArchived)
            .SumAsync(cp => cp.UnreadCount);

        return ResponseModel<UnreadCountDto>.SuccessResponse(new UnreadCountDto
        {
            TotalUnread = totalUnread
        });
    }

    public async Task<ResponseModel<bool>> ArchiveConversationAsync(Guid conversationId)
    {
        var currentUserId = GetCurrentUserId();

        var participant = await _context.ConversationParticipants
            .FirstOrDefaultAsync(cp => cp.ConversationId == conversationId
                && cp.UserId == currentUserId
                && !cp.IsDeleted);

        if (participant == null)
            throw new BusinessException(BusinessErrorCodes.ChatNotParticipant, "Bu söhb?t? giri? icaz?niz yoxdur.");

        participant.Archive();
        await _context.SaveChangesAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

        public async Task<List<Guid>> GetParticipantUserIdsAsync(Guid conversationId)
        {
            return await _context.ConversationParticipants
                .Where(cp => cp.ConversationId == conversationId && !cp.IsDeleted)
                .Select(cp => cp.UserId)
                .ToListAsync();
        }

        public async Task<ResponseModel<List<ChatUserDto>>> SearchUsersAsync(string? search)
        {
            var currentUserId = GetCurrentUserId();

            var query = _userManager.Users
                .Where(u => u.Id != currentUserId);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchLower = search.Trim().ToLower();
                query = query.Where(u => u.FullName.ToLower().Contains(searchLower)
                    || (u.Email != null && u.Email.ToLower().Contains(searchLower)));
            }

            var users = await query
                .OrderBy(u => u.FullName)
                .Take(20)
                .Select(u => new ChatUserDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    ProfileImageUrl = u.ProfileImageUrl
                })
                .ToListAsync();

            foreach (var user in users)
            {
                user.IsOnline = await _presenceTracker.IsUserOnline(user.Id);
            }

            return ResponseModel<List<ChatUserDto>>.SuccessResponse(users);
        }
    }
