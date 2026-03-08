using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QRCoder;
using Startup_Project.Application.Dtos.QrCode;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Enums;
using System.Text.Json;

namespace Startup_Project.Infrastructure.Concretes.Services;

public class QrTokenService(
    IGenericRepository<CheckInToken> tokenRepository,
    IGenericRepository<Subscription> subscriptionRepository,
    IGenericRepository<CheckInLog> logRepository,
    ILogger<QrTokenService> logger) : IQrTokenService
{
    public async Task<ResponseModel<QrCodeResponseDto>> GenerateQrTokenAsync(Guid userId, Guid gymId)
    {
        var activeSub = await subscriptionRepository.Table
            .AsNoTracking()
            .Include(s => s.Package)
                .ThenInclude(p => p.Gym)
            .Where(s => s.UserId == userId
                && s.PackageId != Guid.Empty
                && s.Status == SubscriptionStatus.Active
                && s.EndDate > DateTime.UtcNow
                && !s.IsDeleted)
            .FirstOrDefaultAsync(s => s.Package.GymId == gymId);

        if (activeSub is null)
            return ResponseModel<QrCodeResponseDto>.FailureResponse(
                "Bu zal üçün aktiv abunəliyiniz yoxdur.");

        var token = new CheckInToken(userId, gymId, activeSub.Id);

        await tokenRepository.AddAsync(token);
        await tokenRepository.SaveAsync();

        var qrPayload = JsonSerializer.Serialize(new
        {
            token = token.Token,
            gymId,
            userId,
            expiresAt = token.ExpiryDate
        });

        var qrBase64 = GenerateQrCodeBase64(qrPayload);

        logger.LogInformation("QR generated — User={UserId}, Gym={GymId}, Token={Token}",
            userId, gymId, token.Token);

        return ResponseModel<QrCodeResponseDto>.SuccessResponse(new QrCodeResponseDto
        {
            Token = token.Token,
            QrCodeBase64 = qrBase64,
            GymId = gymId,
            GymName = activeSub.Package.Gym.Name,
            GymAddress = activeSub.Package.Gym.Address,
            ExpiresAt = token.ExpiryDate,
            ExpiresInSeconds = (int)(token.ExpiryDate - DateTime.UtcNow).TotalSeconds,
            SubscriptionPackageName = activeSub.Package.Name
        });
    }

    public async Task<ResponseModel<List<QrCodeResponseDto>>> GetMyActiveQrCodesAsync(Guid userId)
    {
        var activeSubs = await subscriptionRepository.Table
            .AsNoTracking()
            .Include(s => s.Package)
                .ThenInclude(p => p.Gym)
            .Where(s => s.UserId == userId
                && s.Status == SubscriptionStatus.Active
                && s.EndDate > DateTime.UtcNow
                && !s.IsDeleted)
            .ToListAsync();

        var results = new List<QrCodeResponseDto>();

        foreach (var sub in activeSubs)
        {
            var token = new CheckInToken(userId, sub.Package.GymId, sub.Id);
            await tokenRepository.AddAsync(token);

            var qrPayload = JsonSerializer.Serialize(new
            {
                token = token.Token,
                gymId = sub.Package.GymId,
                userId,
                expiresAt = token.ExpiryDate
            });

            results.Add(new QrCodeResponseDto
            {
                Token = token.Token,
                QrCodeBase64 = GenerateQrCodeBase64(qrPayload),
                GymId = sub.Package.GymId,
                GymName = sub.Package.Gym.Name,
                GymAddress = sub.Package.Gym.Address,
                ExpiresAt = token.ExpiryDate,
                ExpiresInSeconds = (int)(token.ExpiryDate - DateTime.UtcNow).TotalSeconds,
                SubscriptionPackageName = sub.Package.Name
            });
        }

        await tokenRepository.SaveAsync();

        return ResponseModel<List<QrCodeResponseDto>>.SuccessResponse(results);
    }

    public async Task<ResponseModel<QrVerifyResponseDto>> VerifyQrTokenAsync(QrVerifyRequestDto request)
    {
        var tokenData = await tokenRepository.Table
            .Include(t => t.User)
            .Include(t => t.Subscription)
                .ThenInclude(s => s.Package)
                    .ThenInclude(p => p.Gym)
            .FirstOrDefaultAsync(t => t.Token == request.Token && !t.IsDeleted);

        if (tokenData is null)
        {
            logger.LogWarning("QR verify FAILED — Token not found: {Token}", request.Token);
            return ResponseModel<QrVerifyResponseDto>.FailureResponse("QR kod etibarsızdır.");
        }

        var checkInLog = new CheckInLog(tokenData.UserId, request.GymId, tokenData.SubscriptionId, request.Token);

        if (tokenData.IsUsed)
        {
            checkInLog.MarkAsFailed("Bu QR kod artıq istifadə edilib.");
            await logRepository.AddAsync(checkInLog);
            await logRepository.SaveAsync();
            return ResponseModel<QrVerifyResponseDto>.FailureResponse("Bu QR kod artıq istifadə edilib.");
        }

        if (tokenData.IsExpired)
        {
            checkInLog.MarkAsExpired();
            await logRepository.AddAsync(checkInLog);
            await logRepository.SaveAsync();
            return ResponseModel<QrVerifyResponseDto>.FailureResponse("QR kodun vaxtı bitib, yenisini alın.");
        }

        if (tokenData.GymId != request.GymId)
        {
            checkInLog.MarkAsFailed("Bu QR kod başqa zala aiddir.");
            await logRepository.AddAsync(checkInLog);
            await logRepository.SaveAsync();
            return ResponseModel<QrVerifyResponseDto>.FailureResponse("Bu QR kod başqa zala aiddir.");
        }

        var subscription = tokenData.Subscription;
        if (subscription is null || !subscription.IsActive)
        {
            checkInLog.MarkAsFailed("Abunəlik müddəti bitib.");
            await logRepository.AddAsync(checkInLog);
            await logRepository.SaveAsync();
            return ResponseModel<QrVerifyResponseDto>.FailureResponse("İstifadəçinin abunəlik müddəti bitib.");
        }

        tokenData.MarkAsUsed();
        checkInLog.MarkAsSuccess();
        await logRepository.AddAsync(checkInLog);
        await tokenRepository.SaveAsync();

        var daysRemaining = Math.Max(0, (int)(subscription.EndDate - DateTime.UtcNow).TotalDays);

        logger.LogInformation("QR verify SUCCESS — User={User}, Gym={Gym}, Token={Token}",
            tokenData.User?.FullName, subscription.Package?.Gym?.Name, request.Token);

        return ResponseModel<QrVerifyResponseDto>.SuccessResponse(new QrVerifyResponseDto
        {
            IsSuccess = true,
            UserFullName = tokenData.User?.FullName ?? "İstifadəçi",
            UserProfileImage = tokenData.User?.ProfileImagePath,
            GymName = subscription.Package?.Gym?.Name ?? "Zal",
            PackageName = subscription.Package?.Name ?? "Paket",
            CheckInTime = checkInLog.CheckInTime,
            SubscriptionEndDate = subscription.EndDate,
            DaysRemaining = daysRemaining
        });
    }

    public async Task<ResponseModel<bool>> CheckOutAsync(Guid checkInLogId)
    {
        var log = await logRepository.Table
            .FirstOrDefaultAsync(c => c.Id == checkInLogId && c.CheckOutTime == null && !c.IsDeleted);

        if (log is null)
            return ResponseModel<bool>.FailureResponse("Check-in tapılmadı və ya artıq check-out edilib.");

        log.CheckOut();
        await logRepository.SaveAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<List<CheckInHistoryItemDto>>> GetMyCheckInHistoryAsync(
        Guid userId, int page = 1, int pageSize = 20)
    {
        var logs = await logRepository.Table
            .AsNoTracking()
            .Where(c => c.UserId == userId && !c.IsDeleted)
            .Include(c => c.Gym)
            .Include(c => c.Subscription)
                .ThenInclude(s => s!.Package)
            .OrderByDescending(c => c.CheckInTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return ResponseModel<List<CheckInHistoryItemDto>>.SuccessResponse(
            logs.Select(MapToHistoryItem).ToList());
    }

    public async Task<ResponseModel<List<CheckInHistoryItemDto>>> GetGymCheckInsAsync(
        Guid gymId, DateTime? date = null, int page = 1, int pageSize = 50)
    {
        var targetDate = date ?? DateTime.UtcNow.Date;
        var nextDate = targetDate.AddDays(1);

        var logs = await logRepository.Table
            .AsNoTracking()
            .Where(c => c.GymId == gymId
                && c.CheckInTime >= targetDate
                && c.CheckInTime < nextDate
                && !c.IsDeleted)
            .Include(c => c.User)
            .Include(c => c.Gym)
            .Include(c => c.Subscription)
                .ThenInclude(s => s!.Package)
            .OrderByDescending(c => c.CheckInTime)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return ResponseModel<List<CheckInHistoryItemDto>>.SuccessResponse(
            logs.Select(MapToHistoryItem).ToList());
    }

    public async Task<ResponseModel<GymCheckInStatsDto>> GetGymCheckInStatsAsync(Guid gymId)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekAgo = today.AddDays(-7);
        var monthAgo = today.AddDays(-30);

        var stats = await logRepository.Table
            .AsNoTracking()
            .Where(c => c.GymId == gymId && c.Status == CheckInStatus.Success && !c.IsDeleted)
            .GroupBy(_ => 1)
            .Select(g => new GymCheckInStatsDto
            {
                TodayCount = g.Count(c => c.CheckInTime >= today),
                WeekCount = g.Count(c => c.CheckInTime >= weekAgo),
                MonthCount = g.Count(c => c.CheckInTime >= monthAgo),
                CurrentlyInGym = g.Count(c => c.CheckInTime >= today && c.CheckOutTime == null)
            })
            .FirstOrDefaultAsync();

        return ResponseModel<GymCheckInStatsDto>.SuccessResponse(
            stats ?? new GymCheckInStatsDto());
    }

    private static CheckInHistoryItemDto MapToHistoryItem(CheckInLog c) => new()
    {
        Id = c.Id,
        UserFullName = c.User?.FullName ?? "İstifadəçi",
        UserProfileImage = c.User?.ProfileImagePath,
        GymName = c.Gym?.Name ?? "Zal",
        PackageName = c.Subscription?.Package?.Name,
        CheckInTime = c.CheckInTime,
        CheckOutTime = c.CheckOutTime,
        Status = c.Status.ToString(),
        DurationMinutes = c.CheckOutTime.HasValue
            ? (int)(c.CheckOutTime.Value - c.CheckInTime).TotalMinutes
            : null
    };

    private static string GenerateQrCodeBase64(string payload)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrCodeData);
        var pngBytes = qrCode.GetGraphic(10);
        return $"data:image/png;base64,{Convert.ToBase64String(pngBytes)}";
    }
}
