using Microsoft.Identity.Client;
using Startup_Project.Application.Abstractions.Services;
using Startup_Project.Application.Abstractions.UnitOfWork;
using Startup_Project.Application.Dtos.User;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;

namespace Startup_Project.Persistance.Concretes.Services;

public class AuthService : IAuthService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMailService _mailService;
    public AuthService(IGenericRepository<User> userRepository, IUnitOfWork unitOfWork, IMailService mailService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _mailService = mailService;
    }
    public async Task SendVerificationCodeAsync(string email)
    {
        var user = await _userRepository.GetSingleAsync(u => u.Email == email);
        if (user == null)
        {
            throw new Exception("Bu email ünvanı ilə qeydiyyat tapılmadı!");
        }
        string code = new Random().Next(1000, 9999).ToString();
        user.VerificationCode = code;
        user.CodeExpireDate = DateTime.UtcNow.AddMinutes(5);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();
        await _mailService.SendEmailAsync(user.Email, "Giriş Təsdiq Kodu", $"Sizin kodunuz: <b>{code}</b>");
    }

    public async Task<bool> VerifyCodeAsync(VerifyCodeDto verifyCodeDto)
    {
        var user = await _userRepository.GetSingleAsync(u => u.Email == verifyCodeDto.Email);
        if (user == null || user.VerificationCode != verifyCodeDto.Code || user.CodeExpireDate < DateTime.UtcNow)
        {
            throw new Exception("Kod yanlışdır və ya vaxtı bitib!");
        }
        user.VerificationCode = null;
        user.CodeExpireDate = null;

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }
}
