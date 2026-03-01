using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Abstractions.AI;
using Startup_Project.Application.Dtos.AI;
using Startup_Project.Application.Interfaces;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class AiChatService(
    IApplicationDbContext context,
    IAiProvider aiProvider,
    UserManager<AppUser> userManager,
    IHttpContextAccessor httpContextAccessor) : IAiChatService
{
    private const int MaxHistoryMessages = 20;
    private const int DailyMessageLimit = 50;
    private const int MinMessageIntervalSeconds = 5;

    #region ==================== SYSTEM PROMPT ====================

    private const string SystemPromptRules = """
        S?n "FitZone AI" adl? fitness köm?kçisis?n.

        QAYDALAR:
        1. YALNIZ fitness, idman, qidalanma, sa?laml?q mövzular?nda cavab ver.
        2. Siyas?t, din, proqramla?d?rma v? dig?r mövzulara cavab VERM? — n?zak?tl? "Bu mövzuda köm?k ed? bilmir?m, ancaq fitness bar?sind? sual?n?z varsa buyurun" de.
        3. User hans? dild? yaz?rsa, o dild? cavab ver. Default olaraq Az?rbaycan dilind? cavab ver.
        4. Tibbi diaqnoz QOYMA — ciddi sa?laml?q problemi varsa h?kim? müraci?t etm?yi tövsiy? et.
        5. Cavablar? strukturlu formatda ver (ba?l?qlar, nömr?li siyah?lar, emoji istifad? et).
        6. Q?sa v? konkret cavab ver, laz?ms?z uzun m?tnl?r yazma.
        7. ?stifad?çinin ??xsi m?lumatlar?n? (a?a??da veril?n profil) cavabda istifad? et — f?rdi tövsiy?l?r ver.
        """;

    #endregion

    #region ==================== SEND MESSAGE ====================

    public async Task<ResponseModel<AiChatResponseDto>> SendMessageAsync(AiChatRequestDto request)
    {
        var userId = GetCurrentUserId();

        // Rate limit
        var rateLimitError = await CheckRateLimitAsync(userId);
        if (rateLimitError is not null)
            return ResponseModel<AiChatResponseDto>.FailureResponse(rateLimitError);

        // Conversation
        AiConversation conversation;
        if (request.ConversationId is null)
        {
            var title = request.Message.Length > 50 ? request.Message[..50] : request.Message;
            conversation = new AiConversation(userId, title);
            context.AiConversations.Add(conversation);
        }
        else
        {
            conversation = await context.AiConversations
                .FirstOrDefaultAsync(c => c.Id == request.ConversationId.Value
                    && c.UserId == userId
                    && c.IsActive)
                ?? throw BusinessException.ValidationFailed("Söhb?t tap?lmad?.");
        }

        // User context
        var userContext = await BuildUserContextAsync(userId);

        // System prompt
        var systemPrompt = string.IsNullOrEmpty(userContext)
            ? SystemPromptRules
            : $"{SystemPromptRules}\n\n{userContext}";

        // History
        var history = await context.AiMessages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversation.Id)
            .OrderByDescending(m => m.CreatedAt)
            .Take(MaxHistoryMessages)
            .Select(m => new AiChatMessage { Role = m.Role, Content = m.Content })
            .ToListAsync();

        history.Reverse();


        // Build messages array
        var messages = new List<AiChatMessage>
        {
            new() { Role = "system", Content = systemPrompt }
        };
        messages.AddRange(history);
        messages.Add(new AiChatMessage { Role = "user", Content = request.Message });

        // Call AI
        var aiResponse = await aiProvider.SendAsync(messages);
        if (!aiResponse.Success)
            return ResponseModel<AiChatResponseDto>.FailureResponse(aiResponse.Error ?? "AI x?tas?.");

        // Save messages
        var userMessage = new AiMessage(conversation.Id, "user", request.Message);
        var assistantMessage = new AiMessage(conversation.Id, "assistant", aiResponse.Content, aiResponse.TokensUsed);

        context.AiMessages.Add(userMessage);
        context.AiMessages.Add(assistantMessage);

        await context.SaveChangesAsync();

        return ResponseModel<AiChatResponseDto>.SuccessResponse(new AiChatResponseDto
        {
            ConversationId = conversation.Id,
            MessageId = assistantMessage.Id,
            Reply = aiResponse.Content,
            CreatedAt = assistantMessage.CreatedAt
        });
    }

    #endregion

    #region ==================== GET CONVERSATIONS ====================

    public async Task<ResponseModel<List<AiConversationListDto>>> GetConversationsAsync()
    {
        var userId = GetCurrentUserId();

        var conversations = await context.AiConversations
            .AsNoTracking()
            .Where(c => c.UserId == userId && c.IsActive)
            .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
            .Select(c => new AiConversationListDto
            {
                Id = c.Id,
                Title = c.Title,
                LastMessage = c.Messages
                    .OrderByDescending(m => m.CreatedAt)
                    .Select(m => m.Content.Length > 100 ? m.Content.Substring(0, 100) + "..." : m.Content)
                    .FirstOrDefault() ?? "",
                MessageCount = c.Messages.Count,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt
            })
            .ToListAsync();

        return ResponseModel<List<AiConversationListDto>>.SuccessResponse(conversations);
    }

    #endregion

    #region ==================== GET MESSAGES ====================

    public async Task<ResponseModel<AiMessageListDto>> GetMessagesAsync(Guid conversationId, int page, int pageSize)
    {
        var userId = GetCurrentUserId();

        var conversation = await context.AiConversations
            .AsNoTracking()
            .AnyAsync(c => c.Id == conversationId && c.UserId == userId && c.IsActive);

        if (!conversation)
            return ResponseModel<AiMessageListDto>.FailureResponse("Söhb?t tap?lmad?.");

        var totalCount = await context.AiMessages
            .CountAsync(m => m.ConversationId == conversationId);

        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

        var messages = await context.AiMessages
            .AsNoTracking()
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new AiMessageDto
            {
                Id = m.Id,
                Role = m.Role,
                Content = m.Content,
                CreatedAt = m.CreatedAt
            })
            .ToListAsync();

        return ResponseModel<AiMessageListDto>.SuccessResponse(new AiMessageListDto
        {
            Messages = messages,
            TotalCount = totalCount,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages
        });
    }

    #endregion

    #region ==================== DELETE CONVERSATION ====================

    public async Task<ResponseModel<bool>> DeleteConversationAsync(Guid conversationId)
    {
        var userId = GetCurrentUserId();

        var conversation = await context.AiConversations
            .FirstOrDefaultAsync(c => c.Id == conversationId && c.UserId == userId);

        if (conversation is null)
            return ResponseModel<bool>.FailureResponse("Söhb?t tap?lmad?.");

        conversation.Deactivate();
        await context.SaveChangesAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    #endregion

    #region ==================== USER CONTEXT BUILDER ====================

    private async Task<string> BuildUserContextAsync(Guid userId)
    {
        var sb = new StringBuilder();
        sb.AppendLine("?ST?FAD?Ç? PROF?L?:");

        // Profile
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null) return string.Empty;

        sb.AppendLine($"- Ad: {user.FullName}");

        var registrationDays = (DateTime.UtcNow - user.CreatedAt).Days;
        if (registrationDays < 30)
            sb.AppendLine($"- Qeydiyyat: {registrationDays} gün ?vv?l");
        else
            sb.AppendLine($"- Qeydiyyat: {registrationDays / 30} ay ?vv?l");

        // Active Subscription (only summary stats, no personal data)
        var now = DateTime.UtcNow;
        var subscription = await context.Subscriptions
            .AsNoTracking()
            .Where(s => s.UserId == userId
                && !s.IsDeleted
                && s.Status == SubscriptionStatus.Active
                && s.EndDate >= now)
            .Include(s => s.Package)
                .ThenInclude(p => p.Gym)
            .OrderByDescending(s => s.EndDate)
            .FirstOrDefaultAsync();

        if (subscription is not null)
        {
            var daysLeft = Math.Max(0, (int)(subscription.EndDate - now).TotalDays);
            sb.AppendLine($"- Abun?lik: {subscription.Package.Name} ({subscription.Package.Gym.Name}), {daysLeft} gün qal?b, Active");
        }
        else
        {
            sb.AppendLine("- Abun?lik: Yoxdur");
        }

        // Check-in stats (only aggregated stats — never raw data)
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var monthlyVisits = await context.CheckInLogs
            .CountAsync(c => c.UserId == userId
                && c.CheckInTime >= startOfMonth
                && c.Status == CheckInStatus.Success);

        sb.AppendLine($"- Bu ay gym ziyar?tl?ri: {monthlyVisits} d?f?");

        var lastCheckIn = await context.CheckInLogs
            .Where(c => c.UserId == userId && c.Status == CheckInStatus.Success)
            .OrderByDescending(c => c.CheckInTime)
            .Select(c => c.CheckInTime)
            .FirstOrDefaultAsync();

        if (lastCheckIn != default)
        {
            var daysSince = (int)(now - lastCheckIn).TotalDays;
            sb.AppendLine(daysSince == 0
                ? "- Son gym ziyar?ti: Bu gün"
                : $"- Son gym ziyar?ti: {daysSince} gün ?vv?l");
        }

        // Trainer (only name + specialization — no contact info)
        var trainerStudent = await context.TrainerStudents
            .AsNoTracking()
            .Where(ts => ts.StudentId == userId && !ts.IsDeleted)
            .Include(ts => ts.Trainer)
                .ThenInclude(t => t.User)
            .FirstOrDefaultAsync();

        if (trainerStudent is not null)
        {
            var trainer = trainerStudent.Trainer;
            sb.AppendLine($"- Trainer: {trainer.User.FullName} ({trainer.Specialization}), Rating: {trainer.AverageRating:F1}");
        }

        // Upcoming session (only next one — not all)
        var nextSession = await context.ScheduledSessions
            .AsNoTracking()
            .Where(s => s.MemberId == userId
                && s.ScheduledTime >= now
                && s.Status != ScheduledSessionStatus.Cancelled
                && !s.IsDeleted)
            .OrderBy(s => s.ScheduledTime)
            .Select(s => new { s.ScheduledTime, s.ExerciseType })
            .FirstOrDefaultAsync();

        if (nextSession is not null)
        {
            var diff = (int)(nextSession.ScheduledTime.Date - now.Date).TotalDays;
            var dateLabel = diff switch
            {
                0 => "Bu gün",
                1 => "Sabah",
                _ => nextSession.ScheduledTime.ToString("dd MMM")
            };
            sb.AppendLine($"- G?l?n sessiya: {dateLabel} {nextSession.ScheduledTime:HH:mm} - {nextSession.ExerciseType}");
        }

        // Session stats this month
        var monthlySessionCount = await context.ScheduledSessions
            .CountAsync(s => s.MemberId == userId
                && s.ScheduledTime >= startOfMonth
                && s.Status != ScheduledSessionStatus.Cancelled
                && !s.IsDeleted);

        var completedSessions = await context.ScheduledSessions
            .CountAsync(s => s.MemberId == userId
                && s.ScheduledTime >= startOfMonth
                && s.Status == ScheduledSessionStatus.Completed
                && !s.IsDeleted);

        if (monthlySessionCount > 0)
        {
            sb.AppendLine($"- Bu ay sessiya: {monthlySessionCount} (tamamlanm??: {completedSessions})");
        }

        return sb.ToString();
    }

    #endregion

    #region ==================== RATE LIMITING ====================

    private async Task<string?> CheckRateLimitAsync(Guid userId)
    {
        // Min interval check
        var lastMessage = await context.AiMessages
            .Where(m => m.Conversation.UserId == userId && m.Role == "user")
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => m.CreatedAt)
            .FirstOrDefaultAsync();

        if (lastMessage != default)
        {
            var elapsed = (DateTime.UtcNow - lastMessage).TotalSeconds;
            if (elapsed < MinMessageIntervalSeconds)
                return $"Çox tez-tez mesaj gönd?rirsiniz. {MinMessageIntervalSeconds} saniy? gözl?yin.";
        }

        // Daily limit check
        var today = DateTime.UtcNow.Date;
        var todayCount = await context.AiMessages
            .CountAsync(m => m.Conversation.UserId == userId
                && m.Role == "user"
                && m.CreatedAt >= today);

        if (todayCount >= DailyMessageLimit)
            return $"Günd?lik mesaj limiti ({DailyMessageLimit}) dolub. Sabah yenid?n c?hd edin.";

        return null;
    }

    #endregion

    #region ==================== HELPERS ====================

    private Guid GetCurrentUserId()
    {
        var claim = httpContextAccessor.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var userId))
            throw BusinessException.UnauthorizedAccess();

        return userId;
    }

    #endregion
}
