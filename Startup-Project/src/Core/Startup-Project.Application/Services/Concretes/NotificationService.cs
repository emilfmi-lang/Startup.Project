using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Abstractions.Notification;
using Startup_Project.Application.Dtos.Notification;
using Startup_Project.Application.Interfaces;
using Startup_Project.Application.Models;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums.Notifications;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes;

public class NotificationService(
    IApplicationDbContext context,
    IMapper mapper,
    IHttpContextAccessor httpContextAccessor) : INotificationService
{
    #region ==================== USER ENDPOINTS ====================

    public async Task<ResponseModel<NotificationListDto>> GetMyNotificationsAsync(int page, int pageSize)
    {
        var userId = GetCurrentUserId();

        var query = context.Notifications
            .Where(n => n.UserId == userId && !n.IsDeleted)
            .OrderByDescending(n => n.CreatedAt);

        var totalCount = await query.CountAsync();
        var unreadCount = await query.CountAsync(n => n.Status != NotificationStatus.Read);

        var notifications = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var dtos = mapper.Map<List<NotificationDto>>(notifications);

        return ResponseModel<NotificationListDto>.SuccessResponse(new NotificationListDto
        {
            Notifications = dtos,
            TotalCount = totalCount,
            UnreadCount = unreadCount,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        });
    }

    public async Task<ResponseModel<int>> GetUnreadCountAsync()
    {
        var userId = GetCurrentUserId();

        var count = await context.Notifications
            .CountAsync(n => n.UserId == userId
                && !n.IsDeleted
                && n.Status != NotificationStatus.Read);

        return ResponseModel<int>.SuccessResponse(count);
    }

    public async Task<ResponseModel<bool>> MarkAsReadAsync(Guid notificationId)
    {
        var userId = GetCurrentUserId();

        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && !n.IsDeleted);

        if (notification is null)
            return ResponseModel<bool>.FailureResponse("Bildiri? tap?lmad?.");

        notification.MarkAsRead();
        await context.SaveChangesAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> MarkAllAsReadAsync()
    {
        var userId = GetCurrentUserId();

        var unreadNotifications = await context.Notifications
            .Where(n => n.UserId == userId && !n.IsDeleted && n.Status != NotificationStatus.Read)
            .ToListAsync();

        foreach (var notification in unreadNotifications)
            notification.MarkAsRead();

        await context.SaveChangesAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> DeleteAsync(Guid notificationId)
    {
        var userId = GetCurrentUserId();

        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId && !n.IsDeleted);

        if (notification is null)
            return ResponseModel<bool>.FailureResponse("Bildiri? tap?lmad?.");

        notification.IsDeleted = true;
        await context.SaveChangesAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    #endregion

    #region ==================== DEVICE MANAGEMENT ====================

    public async Task<ResponseModel<bool>> RegisterDeviceAsync(RegisterDeviceDto dto)
    {
        var userId = GetCurrentUserId();

        var existing = await context.UserDevices
            .FirstOrDefaultAsync(d => d.DeviceToken == dto.DeviceToken && d.UserId == userId);

        if (existing is not null)
            return ResponseModel<bool>.SuccessResponse(true);

        var device = new UserDevice(userId, dto.DeviceToken, dto.Platform, dto.DeviceModel, dto.AppVersion);
        context.UserDevices.Add(device);
        await context.SaveChangesAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> UnregisterDeviceAsync(string deviceToken)
    {
        var userId = GetCurrentUserId();

        var device = await context.UserDevices
            .FirstOrDefaultAsync(d => d.DeviceToken == deviceToken && d.UserId == userId);

        if (device is not null)
        {
            device.IsDeleted = true;
            await context.SaveChangesAsync();
        }

        return ResponseModel<bool>.SuccessResponse(true);
    }

    #endregion

    #region ==================== INTERNAL (OTHER SERVICES CALL) ====================

    public async Task SendAsync(
        Guid userId,
        NotificationType type,
        string title,
        string body,
        NotificationChannel channel = NotificationChannel.InApp,
        string? imageUrl = null,
        string? actionUrl = null,
        Guid? relatedEntityId = null,
        string? relatedEntityType = null)
    {
        var notification = new Notification(
            userId, title, body, type, channel,
            imageUrl, actionUrl, null,
            relatedEntityId, relatedEntityType);

        notification.MarkAsSent();

        context.Notifications.Add(notification);
        await context.SaveChangesAsync();
    }

    public async Task SendBulkAsync(
        List<Guid> userIds,
        NotificationType type,
        string title,
        string body,
        NotificationChannel channel = NotificationChannel.InApp)
    {
        var notifications = userIds.Select(userId =>
        {
            var notification = new Notification(userId, title, body, type, channel);
            notification.MarkAsSent();
            return notification;
        }).ToList();

        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync();
    }

    #endregion

    #region ==================== ADMIN ====================

    public async Task<ResponseModel<Guid>> AdminSendAsync(SendNotificationDto dto)
    {
        if (dto.UserId is null && (dto.UserIds is null || dto.UserIds.Count == 0))
            return ResponseModel<Guid>.FailureResponse("UserId v? ya UserIds t?yin olunmal?d?r.");

        if (dto.UserId.HasValue)
        {
            var notification = new Notification(
                dto.UserId.Value, dto.Title, dto.Body, dto.Type, dto.Channel,
                dto.ImageUrl, dto.ActionUrl);

            if (dto.ScheduledFor.HasValue)
                notification.Schedule(dto.ScheduledFor.Value);
            else
                notification.MarkAsSent();

            context.Notifications.Add(notification);
            await context.SaveChangesAsync();

            return ResponseModel<Guid>.SuccessResponse(notification.Id);
        }

        // Multiple users
        foreach (var userId in dto.UserIds!)
        {
            var notification = new Notification(
                userId, dto.Title, dto.Body, dto.Type, dto.Channel,
                dto.ImageUrl, dto.ActionUrl);

            if (dto.ScheduledFor.HasValue)
                notification.Schedule(dto.ScheduledFor.Value);
            else
                notification.MarkAsSent();

            context.Notifications.Add(notification);
        }

        await context.SaveChangesAsync();
        return ResponseModel<Guid>.SuccessResponse(Guid.Empty);
    }

    public async Task<ResponseModel<bool>> AdminBroadcastAsync(string title, string body, NotificationType type)
    {
        // TODO: Implement broadcast to all users
        return ResponseModel<bool>.SuccessResponse(true);
    }

    #endregion

    #region ==================== PRIVATE HELPERS ====================

    private Guid GetCurrentUserId()
    {
        var claim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var userId))
            throw BusinessException.UnauthorizedAccess();
        return userId;
    }

    #endregion
}
