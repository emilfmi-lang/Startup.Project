using Startup_Project.Application.Abstractions.UnitOfWork;
using Startup_Project.Application.Models;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Infrastructure.Concretes.Services;

public class QrTokenService
{
    private readonly IGenericRepository<CheckInToken> _tokenRepository;
    private readonly IGenericRepository<Subscription> _subscriptionRepository;
    private readonly IGenericRepository<CheckInLog> _logRepository;
    private readonly IUnitOfWork _unitOfWork;

    public QrTokenService(
        IGenericRepository<CheckInToken> tokenRepository,
        IGenericRepository<Subscription> subscriptionRepository,
        IGenericRepository<CheckInLog> logRepository,
        IUnitOfWork unitOfWork)
    {
        _tokenRepository = tokenRepository;
        _subscriptionRepository = subscriptionRepository;
        _logRepository = logRepository;
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

        if (tokenData.IsUsed)
            return ResponseModel<bool>.FailureResponse("Bu QR kod artıq istifadə edilib.");

        if (tokenData.ExpiryDate < DateTime.Now)
            return ResponseModel<bool>.FailureResponse("QR kodun vaxtı bitib, yenisini alın.");

        var isSubValid = await _subscriptionRepository
            .AnyAsync(s => s.UserId == tokenData.UserId && s.EndDate > DateTime.Now);

        if (!isSubValid)
            return ResponseModel<bool>.FailureResponse("İstifadəçinin abunəlik müddəti artıq bitib.");

        tokenData.IsUsed = true; 

        var checkInLog = new CheckInLog
        {
            UserId = tokenData.UserId,
            CheckInTime = DateTime.Now
        };

        await _logRepository.AddAsync(checkInLog);
        await _unitOfWork.SaveChangesAsync();

        return ResponseModel<bool>.SuccessResponse(true);
    }
}
