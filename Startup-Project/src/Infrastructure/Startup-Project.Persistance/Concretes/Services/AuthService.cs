using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;
using Startup_Project.Application.Abstractions.Services;
using Startup_Project.Application.Abstractions.UnitOfWork;
using Startup_Project.Application.Dtos.Token;
using Startup_Project.Application.Dtos.User;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Entities.Identity;

namespace Startup_Project.Persistance.Concretes.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _manager;
    private readonly IMailService _mailService;
    private readonly ITokenHandler _tokenHandler;
    public AuthService(IMailService mailService, ITokenHandler tokenHandler,
                       UserManager<AppUser> manager)
    {
        _mailService = mailService;
        _tokenHandler = tokenHandler;
        _manager = manager;
    }
    public async Task<bool> SendVerificationCodeAsync(string email)
    {
        var user = await _manager.FindByEmailAsync(email);
        if (user == null)
        {
            throw new Exception("Bu email ünvanı ilə qeydiyyat tapılmadı!");
        }
        string code = new Random().Next(1000, 9999).ToString();
        user.VerificationCode = code;
        user.CodeExpireDate = DateTime.UtcNow.AddMinutes(5);
        var result = await _manager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            throw new Exception("Kod göndərilmədi, zəhmət olmasa yenidən cəhd edin!");
        }
        await _mailService.SendEmailAsync(user.Email, "Giriş Təsdiq Kodu", $"Sizin kodunuz: <b>{code}</b>");
        return true;
    }

    public async Task<TokenDto> VerifyCodeAsync(VerifyCodeDto verifyCodeDto)
    {
        var user = await _manager.FindByEmailAsync(verifyCodeDto.Email);
        if (user == null)
            throw new Exception("İstifadəçi tapılmadı!");
        if (user.VerificationCode != verifyCodeDto.Code) throw new Exception("Kod yanlışdır!");
        if (user.CodeExpireDate < DateTime.UtcNow) throw new Exception("Kodun vaxtı bitib!");
        user.VerificationCode = null;
        user.CodeExpireDate = null;
        await _manager.UpdateAsync(user);
        return await _tokenHandler.CreateAccessToken(user);
    }
}
