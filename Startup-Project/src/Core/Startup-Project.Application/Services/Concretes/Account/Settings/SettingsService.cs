using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Abstractions.UnitOfWork;
using Startup_Project.Application.Dtos.Account;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Application.Services.Interfaces.Account.Settings;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Exceptions;
using System.Security.Claims;

namespace Startup_Project.Application.Services.Concretes.Account.Settings;

public class SettingsService(
    UserManager<AppUser> userManager,
    IGenericRepository<UserSession> sessionRepository,
    IGenericRepository<UserPaymentMethod> paymentMethodRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    IFileService fileService,
    IHttpContextAccessor httpContextAccessor) : ISettingsService
{
    private static readonly string[] AllowedPhotoExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxPhotoSizeBytes = 5 * 1024 * 1024; // 5MB
    private const int MaxPaymentMethods = 5;

    #region ═══════════════════ OVERVIEW ═══════════════════

    public async Task<ResponseModel<SettingsOverviewDto>> GetSettingsOverviewAsync()
    {
        var user = await GetCurrentUserAsync();

        var sessionsCount = await sessionRepository.Table
            .CountAsync(s => s.UserId == user.Id);

        var paymentMethodsCount = await paymentMethodRepository.Table
            .CountAsync(pm => pm.UserId == user.Id);

        return ResponseModel<SettingsOverviewDto>.SuccessResponse(new SettingsOverviewDto
        {
            Profile = MapToProfileDto(user),
            Notifications = mapper.Map<NotificationSettingsDto>(user),
            Preferences = mapper.Map<UserPreferencesDto>(user),
            ActiveSessionsCount = sessionsCount,
            PaymentMethodsCount = paymentMethodsCount
        });
    }

    #endregion

    #region ═══════════════════ PROFILE ═══════════════════

    public async Task<ResponseModel<UserProfileDto>> GetProfileAsync()
    {
        var user = await GetCurrentUserAsync();
        return ResponseModel<UserProfileDto>.SuccessResponse(MapToProfileDto(user));
    }

    public async Task<ResponseModel<UserProfileDto>> UpdateProfileAsync(UpdateProfileDto dto)
    {
        var user = await GetCurrentUserAsync();

        if (!string.IsNullOrWhiteSpace(dto.FullName))
            user.FullName = dto.FullName;

        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
        {
            if (user.AuthProvider == AuthProvider.Google)
                throw BusinessException.ValidationFailed("Google hesabı ilə qeydiyyatdan keçən istifadəçi email dəyişə bilməz.");

            var existing = await userManager.FindByEmailAsync(dto.Email);
            if (existing != null && existing.Id != user.Id)
                throw BusinessException.ValidationFailed("Bu email artıq istifadə olunur.");

            user.Email = dto.Email;
            user.UserName = dto.Email;
        }

        if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
            user.PhoneNumber = dto.PhoneNumber;

        if (dto.Bio != null)
            user.Bio = dto.Bio;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw BusinessException.ValidationFailed(string.Join(", ", result.Errors.Select(e => e.Description)));

        return ResponseModel<UserProfileDto>.SuccessResponse(MapToProfileDto(user));
    }

    public async Task<ResponseModel<string>> ChangePhotoAsync(IFormFile photo)
    {
        ValidatePhoto(photo);

        var user = await GetCurrentUserAsync();

        if (!string.IsNullOrEmpty(user.ProfileImagePath))
            await fileService.DeleteFileAsync(user.ProfileImagePath);

        var path = await fileService.UploadFileAsync(photo, "avatars");
        user.ProfileImagePath = path;
        user.ProfileImageUrl = path;

        await userManager.UpdateAsync(user);

        return ResponseModel<string>.SuccessResponse(path);
    }

    public async Task<ResponseModel<bool>> RemovePhotoAsync()
    {
        var user = await GetCurrentUserAsync();

        if (!string.IsNullOrEmpty(user.ProfileImagePath))
            await fileService.DeleteFileAsync(user.ProfileImagePath);

        user.ProfileImagePath = null;
        user.ProfileImageUrl = null;

        await userManager.UpdateAsync(user);

        return ResponseModel<bool>.SuccessResponse(true);
    }

    #endregion

    #region ═══════════════════ SECURITY ═══════════════════

    public async Task<ResponseModel<bool>> ChangePasswordAsync(ChangePasswordDto dto)
    {
        var user = await GetCurrentUserAsync();

        if (user.AuthProvider != AuthProvider.Local)
            throw BusinessException.PasswordResetNotAllowed(user.AuthProvider.ToString());

        var checkResult = await userManager.CheckPasswordAsync(user, dto.CurrentPassword);
        if (!checkResult)
            throw BusinessException.CurrentPasswordIncorrect();

        var result = await userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
            throw BusinessException.ValidationFailed(string.Join(", ", result.Errors.Select(e => e.Description)));

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<ActiveSessionsResponseDto>> GetActiveSessionsAsync()
    {
        var userId = GetCurrentUserId();

        var sessions = await sessionRepository.Table
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.IsCurrent)
            .ThenByDescending(s => s.LastActiveAt)
            .ToListAsync();

        var dtos = sessions.Select(s =>
        {
            var dto = mapper.Map<ActiveSessionDto>(s);
            dto.LastActiveLabel = FormatLastActive(s.LastActiveAt);
            return dto;
        }).ToList();

        return ResponseModel<ActiveSessionsResponseDto>.SuccessResponse(new ActiveSessionsResponseDto
        {
            Sessions = dtos,
            TotalCount = dtos.Count
        });
    }

    public async Task<ResponseModel<bool>> RevokeSessionAsync(Guid sessionId)
    {
        var userId = GetCurrentUserId();

            var session = await sessionRepository.Table
                .FirstOrDefaultAsync(s => s.Id == sessionId && s.UserId == userId);

            if (session == null)
                throw BusinessException.SessionNotFoundForRevoke();

            if (session.IsCurrent)
                throw BusinessException.CannotRevokeCurrentSession();

            session.Revoke();
            sessionRepository.Update(session);
            await unitOfWork.SaveChangesAsync();

            return ResponseModel<bool>.SuccessResponse(true);
        }

        public async Task<ResponseModel<bool>> RevokeAllOtherSessionsAsync()
        {
            var userId = GetCurrentUserId();

            var otherSessions = await sessionRepository.Table
                .Where(s => s.UserId == userId && !s.IsCurrent)
                .ToListAsync();

            foreach (var session in otherSessions)
                session.Revoke();

            await unitOfWork.SaveChangesAsync();

            return ResponseModel<bool>.SuccessResponse(true);
        }

        #endregion

        #region ═══════════════════ NOTIFICATIONS ═══════════════════

    public async Task<ResponseModel<NotificationSettingsDto>> GetNotificationSettingsAsync()
    {
        var user = await GetCurrentUserAsync();
        return ResponseModel<NotificationSettingsDto>.SuccessResponse(mapper.Map<NotificationSettingsDto>(user));
    }

    public async Task<ResponseModel<NotificationSettingsDto>> UpdateNotificationSettingsAsync(NotificationSettingsDto dto)
    {
        var user = await GetCurrentUserAsync();

        user.NotifyPush = dto.PushNotifications;
        user.NotifyEmail = dto.EmailNotifications;
        user.NotifySms = dto.SmsNotifications;
        user.NotifyWorkoutReminders = dto.WorkoutReminders;
        user.NotifyPromotions = dto.PromotionalOffers;
        user.NotifySessionUpdates = dto.SessionUpdates;
        user.NotifyNewFeatures = dto.NewFeatures;

        await userManager.UpdateAsync(user);

        return ResponseModel<NotificationSettingsDto>.SuccessResponse(mapper.Map<NotificationSettingsDto>(user));
    }

    #endregion

    #region ═══════════════════ PREFERENCES ═══════════════════

    public async Task<ResponseModel<UserPreferencesDto>> GetPreferencesAsync()
    {
        var user = await GetCurrentUserAsync();
        return ResponseModel<UserPreferencesDto>.SuccessResponse(mapper.Map<UserPreferencesDto>(user));
    }

    public async Task<ResponseModel<UserPreferencesDto>> UpdatePreferencesAsync(UserPreferencesDto dto)
    {
        var user = await GetCurrentUserAsync();

        user.Language = dto.Language;
        user.Theme = dto.Theme;
        user.DistanceUnit = dto.DistanceUnit;
        user.TimeZone = dto.TimeZone;

        await userManager.UpdateAsync(user);

        return ResponseModel<UserPreferencesDto>.SuccessResponse(mapper.Map<UserPreferencesDto>(user));
    }

    #endregion

    #region ═══════════════════ PAYMENT METHODS ═══════════════════

    public async Task<ResponseModel<PaymentMethodsResponseDto>> GetPaymentMethodsAsync()
    {
        var userId = GetCurrentUserId();

        var methods = await paymentMethodRepository.Table
            .Where(pm => pm.UserId == userId)
            .OrderByDescending(pm => pm.IsDefault)
            .ThenByDescending(pm => pm.CreatedAt)
            .ToListAsync();

        var dtos = mapper.Map<List<PaymentMethodDto>>(methods);

        return ResponseModel<PaymentMethodsResponseDto>.SuccessResponse(new PaymentMethodsResponseDto
        {
            Methods = dtos,
            TotalCount = dtos.Count
        });
    }

    public async Task<ResponseModel<PaymentMethodDto>> AddPaymentMethodAsync(AddPaymentMethodDto dto)
    {
        var userId = GetCurrentUserId();

        var existingCount = await paymentMethodRepository.Table
            .CountAsync(pm => pm.UserId == userId);

        if (existingCount >= MaxPaymentMethods)
            throw BusinessException.PaymentMethodLimitExceeded();

        var sanitized = dto.CardNumber.Replace(" ", "").Replace("-", "");
        var last4 = sanitized[^4..];

        var duplicate = await paymentMethodRepository.Table
            .AnyAsync(pm => pm.UserId == userId && pm.Last4Digits == last4
                && pm.ExpiryMonth == dto.ExpiryMonth && pm.ExpiryYear == dto.ExpiryYear);

        if (duplicate)
            throw BusinessException.PaymentMethodAlreadyExists();

        var cardType = DetectCardType(sanitized);
        var isDefault = dto.IsDefault || existingCount == 0;

        if (isDefault)
            await UnsetAllDefaults(userId);

        var method = new UserPaymentMethod(
            userId, cardType, dto.CardHolderName.Trim(),
            last4, dto.ExpiryMonth, dto.ExpiryYear, isDefault);

        await paymentMethodRepository.AddAsync(method);
        await unitOfWork.SaveChangesAsync();

        return ResponseModel<PaymentMethodDto>.SuccessResponse(mapper.Map<PaymentMethodDto>(method));
    }

    public async Task<ResponseModel<bool>> RemovePaymentMethodAsync(Guid methodId)
    {
        var userId = GetCurrentUserId();

            var method = await paymentMethodRepository.Table
                .FirstOrDefaultAsync(pm => pm.Id == methodId && pm.UserId == userId);

            if (method == null)
                throw BusinessException.PaymentMethodNotFound();

            var othersExist = await paymentMethodRepository.Table
                .AnyAsync(pm => pm.UserId == userId && pm.Id != methodId);

            if (method.IsDefault && othersExist)
                throw BusinessException.CannotRemoveDefaultPaymentMethod();

            method.Remove();
            paymentMethodRepository.Update(method);
            await unitOfWork.SaveChangesAsync();

            return ResponseModel<bool>.SuccessResponse(true);
        }

        public async Task<ResponseModel<bool>> SetDefaultPaymentMethodAsync(Guid methodId)
        {
            var userId = GetCurrentUserId();

            var method = await paymentMethodRepository.Table
                .FirstOrDefaultAsync(pm => pm.Id == methodId && pm.UserId == userId);

            if (method == null)
                throw BusinessException.PaymentMethodNotFound();

            await UnsetAllDefaults(userId);

            method.SetAsDefault();
            paymentMethodRepository.Update(method);
            await unitOfWork.SaveChangesAsync();

            return ResponseModel<bool>.SuccessResponse(true);
        }

        #endregion

        #region ═══════════════════ DANGER ZONE ═══════════════════

    public async Task<ResponseModel<bool>> DeleteAccountAsync()
    {
        var user = await GetCurrentUserAsync();

        // Bütün sessiyaları ləğv et
        var sessions = await sessionRepository.Table
            .Where(s => s.UserId == user.Id)
            .ToListAsync();

        foreach (var session in sessions)
            session.Revoke();

        // Ödəniş metodlarını sil
        var paymentMethods = await paymentMethodRepository.Table
            .Where(pm => pm.UserId == user.Id)
            .ToListAsync();

        foreach (var pm in paymentMethods)
            pm.Remove();

        await unitOfWork.SaveChangesAsync();

        // Soft delete — hesab silinmiş kimi qeyd olunur, DB-dən fiziki silinmir
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.DeletionReason = "User tərəfindən silinib";
        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
            throw BusinessException.ValidationFailed("Hesab silinməsi uğursuz oldu.");

        return ResponseModel<bool>.SuccessResponse(true);
    }

    #endregion

    #region ═══════════════════ HELPERS ═══════════════════

    private Guid GetCurrentUserId()
    {
        var claim = httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(claim) || !Guid.TryParse(claim, out var userId))
            throw BusinessException.UnauthorizedAccess();
        return userId;
    }

    private async Task<AppUser> GetCurrentUserAsync()
    {
        var userId = GetCurrentUserId();
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            throw BusinessException.UnauthorizedAccess();
        return user;
    }

    private static UserProfileDto MapToProfileDto(AppUser user) => new()
    {
        Id = user.Id,
        FullName = user.FullName,
        Email = user.Email!,
        PhoneNumber = user.PhoneNumber,
        Bio = user.Bio,
        ProfileImageUrl = user.ProfileImageUrl,
        AuthProvider = user.AuthProvider.ToString(),
        JoinedAt = user.CreatedAt
    };

    private static void ValidatePhoto(IFormFile photo)
    {
        var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
        if (!AllowedPhotoExtensions.Contains(extension))
            throw BusinessException.InvalidPhotoFormat();

        if (photo.Length > MaxPhotoSizeBytes)
            throw BusinessException.PhotoSizeTooLarge();
    }

    private static string FormatLastActive(DateTime lastActive)
    {
        var diff = DateTime.UtcNow - lastActive;

        if (diff.TotalMinutes < 2) return "Now";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes} minutes ago";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours} hours ago";
        if (diff.TotalDays < 7) return $"{(int)diff.TotalDays} days ago";
        return lastActive.ToString("MMM dd, yyyy");
    }

    private async Task UnsetAllDefaults(Guid userId)
    {
        var defaults = await paymentMethodRepository.Table
            .Where(pm => pm.UserId == userId && pm.IsDefault)
            .ToListAsync();

        foreach (var d in defaults)
            d.UnsetDefault();
    }

    private static CardType DetectCardType(string number)
    {
        if (number.StartsWith('4')) return CardType.Visa;
        if (number.StartsWith("51") || number.StartsWith("52") || number.StartsWith("53") ||
            number.StartsWith("54") || number.StartsWith("55") || (number.Length >= 4 &&
            int.TryParse(number[..4], out var mc) && mc >= 2221 && mc <= 2720))
            return CardType.MasterCard;
        if (number.StartsWith("34") || number.StartsWith("37")) return CardType.AmericanExpress;
        if (number.StartsWith("62")) return CardType.UnionPay;
        return CardType.Other;
    }

    #endregion
}
