using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Startup_Project.Application.Abstractions.Services;
using Startup_Project.Application.Dtos.Account;
using Startup_Project.Application.Dtos.Account.Password;
using Startup_Project.Application.Dtos.User;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces.Account;
using Startup_Project.Application.Validators.Account;
using Startup_Project.Domain.Entities.Identity;
using System.Net.NetworkInformation;


namespace Startup_Project.Application.Services.Concretes.Account;

public class AccountService(UserManager<AppUser> userManager,
    IMapper mapper,
    IJwtTokenService jwtTokenService,IMailService mailService
    ) : IAccountService
{
    public async Task<ResponseModel<string>> VerifyAndLoginAsync(VerifyCodeDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return ResponseModel<string>.FailureResponse("İstifadəçi tapılmadı.");
        if(user.VerificationCode != dto.Code)
            return ResponseModel<string>.FailureResponse("Daxil etdiyiniz kod yanlışdır.");
        if(user.CodeExpireDate < DateTime.UtcNow)
            return ResponseModel<string>.FailureResponse("Daxil etdiyiniz kodun etibarlılıq müddəti bitib.");
        user.EmailConfirmed = true;
        user.VerificationCode = null;
        user.CodeExpireDate = null;
        await userManager.UpdateAsync(user);
        var roles = await userManager.GetRolesAsync(user);
        var token = jwtTokenService.GenerateTokenAsync(user, roles);
        return ResponseModel<string>.SuccessResponse(token);
    }

    public async Task<ResponseModel<bool>> RegisterAsync(UserRegisterDto dto)
    {
        var validator = new UserRegisterDtoValidator();
        var validationResult = await validator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            return ResponseModel<bool>.FailureResponse(validationResult.Errors.Select(e => e.ErrorMessage).ToList());
        }
        var user = mapper.Map<AppUser>(dto);
        user.UserName = dto.Email;

        user.VerificationCode = new Random().Next(1000, 9999).ToString();
        user.CodeExpireDate = DateTime.UtcNow.AddMinutes(5);

        var result = await userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return ResponseModel<bool>.FailureResponse(result.Errors.Select(e => e.Description).ToList());
        }
        string mailBody = $@"<h3>Salam {user.FullName},</h3>
                             <p>Qeydiyyatı tamamlamaq üçün təsdiq kodunuz: <b>{user.VerificationCode}</b></p>
                             <p>Bu kod 10 dəqiqə ərzində keçərlidir.</p>";
        await mailService.SendEmailAsync(user.Email!, "Qeydiyyat Təsdiqi", mailBody);
        return ResponseModel<bool>.SuccessResponse(true);

    }
    public async Task<ResponseModel<string>> LoginAsync(UserLoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return ResponseModel<string>.FailureResponse("İstifadəçi tapılmadı.");

        var isPasswordValid = await userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
            return ResponseModel<string>.FailureResponse("Email və ya şifrə yanlışdır.");

        if (!user.EmailConfirmed)
            return ResponseModel<string>.FailureResponse("Zəhmət olmasa əvvəlcə emailinizi təsdiqləyin.");

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtTokenService.GenerateTokenAsync(user, roles);

        return ResponseModel<string>.SuccessResponse(token);
    }

    public async Task<ResponseModel<bool>> ForgotPasswordAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if(user == null)
            return ResponseModel<bool>.FailureResponse("İstifadəçi tapılmadı.");
        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        byte[] tokenBytes = System.Text.Encoding.UTF8.GetBytes(token);
        var encodedToken = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(tokenBytes);
        var callbackUrl= $"https://localhost:7000/api/Account/ResetPassword?email={user.Email}&token={encodedToken}";
        string mailBody = $@"<h3>Şifrə Sıfırlama İstəyi</h3>
                             <p>Salam {user.FullName},</p>
                             <p>Şifrənizi yeniləmək üçün aşağıdakı linkə klikləyin:</p>
                             <a href='{callbackUrl}'>Şifrəni Yenilə</a>
                             <p>Əgər bu istəyi siz etməmisinizsə, bu emaili görməzdən gəlin.</p>";

        await mailService.SendEmailAsync(user.Email!, "Şifrə Sıfırlama", mailBody);

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return ResponseModel<bool>.FailureResponse("İstifadəçi tapılmadı.");
        if(dto.NewPassword != dto.ConfirmPassword)
            return ResponseModel<bool>.FailureResponse("Yeni şifrə və təsdiq şifrəsi uyğun gəlmir.");

        var decodedTokenBytes = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlDecode(dto.Token);
        var orginalToken = System.Text.Encoding.UTF8.GetString(decodedTokenBytes);
        var result = await userManager.ResetPasswordAsync(user, orginalToken, dto.NewPassword);
        if(!result.Succeeded)
            return ResponseModel<bool>.FailureResponse(result.Errors.Select(e => e.Description).ToList());
        return ResponseModel<bool>.SuccessResponse(true);
    }
}
