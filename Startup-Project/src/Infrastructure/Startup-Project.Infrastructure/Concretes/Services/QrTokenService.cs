using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Abstractions.UnitOfWork;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Infrastructure.Concretes.Services;

public class QrTokenService : IQrTokenService
{
    private readonly IGenericRepository<CheckInToken> _tokenRepository;
    private readonly IGenericRepository<Subscription> _subscriptionRepository;
    private readonly IGenericRepository<CheckInLog> _logRepository;
    private readonly IGenericRepository<Package> _packageRepository;
    private readonly IUnitOfWork _unitOfWork;

    public QrTokenService(
        IGenericRepository<CheckInToken> tokenRepository,
        IGenericRepository<Subscription> subscriptionRepository,
        IGenericRepository<CheckInLog> logRepository,
        IGenericRepository<Package> packageRepository,
        IUnitOfWork unitOfWork)
    {
        _tokenRepository = tokenRepository;
        _subscriptionRepository = subscriptionRepository;
        _logRepository = logRepository;
        _packageRepository = packageRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ResponseModel<string>> GenerateQrTokenAsync(Guid userId)
    {
        var activeSub = await _subscriptionRepository
            .GetSingleAsync(s => s.UserId == userId && s.EndDate > DateTime.Now);

        if (activeSub == null)
            return ResponseModel<string>.FailureResponse("Aktiv abunəliyiniz tapılmadı və ya müddəti bitib.");

        var tokenString = Guid.NewGuid().ToString("N"); 

        var checkInToken = new CheckInToken
        {
            Token = tokenString,
            UserId = userId,
            ExpiryDate = DateTime.Now.AddMinutes(5), 
            IsUsed = false
        };

        await _tokenRepository.AddAsync(checkInToken);
        await _unitOfWork.SaveChangesAsync();

        return ResponseModel<string>.SuccessResponse(tokenString);
    }

    public async Task<ResponseModel<bool>> VerifyQrTokenAsync(string token)
    {
        var tokenData = await _tokenRepository.GetSingleAsync(t => t.Token == token);

        if (tokenData == null)
            return ResponseModel<bool>.FailureResponse("QR kod etibarsızdır.");

        // Get user's active subscription with Gym info
        var activeSub = await _subscriptionRepository.Table
            .AsNoTracking()
            .Include(s => s.Package)
            .FirstOrDefaultAsync(s => s.UserId == tokenData.UserId && s.EndDate > DateTime.Now);

        var gymId = activeSub?.Package?.GymId ?? Guid.Empty;

        // Create check-in log regardless of outcome
        var checkInLog = new CheckInLog(tokenData.UserId, gymId, token);

        if (tokenData.IsUsed)
        {
            checkInLog.MarkAsFailed("Bu QR kod artıq istifadə edilib.");
            await _logRepository.AddAsync(checkInLog);
            await _unitOfWork.SaveChangesAsync();
            return ResponseModel<bool>.FailureResponse("Bu QR kod artıq istifadə edilib.");
        }

        if (tokenData.ExpiryDate < DateTime.Now)
        {
            checkInLog.MarkAsExpired();
            await _logRepository.AddAsync(checkInLog);
            await _unitOfWork.SaveChangesAsync();
            return ResponseModel<bool>.FailureResponse("QR kodun vaxtı bitib, yenisini alın.");
        }

        if (activeSub == null)
        {
            checkInLog.MarkAsFailed("İstifadəçinin abunəlik müddəti bitib.");
            await _logRepository.AddAsync(checkInLog);
            await _unitOfWork.SaveChangesAsync();
            return ResponseModel<bool>.FailureResponse("İstifadəçinin abunəlik müddəti artıq bitib.");
        }

        // Success case
        tokenData.IsUsed = true;
        checkInLog.MarkAsSuccess();

        await _logRepository.AddAsync(checkInLog);
        await _unitOfWork.SaveChangesAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }
}
