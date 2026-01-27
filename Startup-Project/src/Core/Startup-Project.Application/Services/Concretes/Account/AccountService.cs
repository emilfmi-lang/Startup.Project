using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Abstractions.Services;
using Startup_Project.Application.Dtos.Account;
using Startup_Project.Application.Dtos.Account.Password;
using Startup_Project.Application.Dtos.User;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces.Account;
using Startup_Project.Application.Validators.Account;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;


namespace Startup_Project.Application.Services.Concretes.Account;

public class AccountService(
    UserManager<AppUser> userManager,
    IMapper mapper,
    IJwtTokenService jwtTokenService,
    IMailService mailService,
    IGoogleTokenValidator googleTokenValidator
    ) : IAccountService
{
    public async Task<ResponseModel<string>> VerifyAndLoginAsync(VerifyCodeDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return ResponseModel<string>.FailureResponse("Istifadeci tapilmadi.");
        if (user.VerificationCode != dto.Code)
            return ResponseModel<string>.FailureResponse("Daxil etdiyiniz kod yanlisdir.");
        if (user.CodeExpireDate < DateTime.UtcNow)
            return ResponseModel<string>.FailureResponse("Daxil etdiyiniz kodun etibarliliq muddeti bitib.");
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
        user.AuthProvider = AuthProvider.Local;

        user.VerificationCode = new Random().Next(1000, 9999).ToString();
        user.CodeExpireDate = DateTime.UtcNow.AddMinutes(5);

        var result = await userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            return ResponseModel<bool>.FailureResponse(result.Errors.Select(e => e.Description).ToList());
        }

        await userManager.AddToRoleAsync(user, "User");

        string mailBody = $@"<h3>Salam {user.FullName},</h3>
                             <p>Qeydiyyati tamamlamaq ucun tesdiq kodunuz: <b>{user.VerificationCode}</b></p>
                             <p>Bu kod 5 deqiqe erzinde kecerlidir.</p>";
        await mailService.SendEmailAsync(user.Email!, "Qeydiyyat Tesdiqi", mailBody);
        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<string>> LoginAsync(UserLoginDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return ResponseModel<string>.FailureResponse("Istifadeci tapilmadi.");

        if (user.AuthProvider == AuthProvider.Google)
            return ResponseModel<string>.FailureResponse("Bu hesab Google ile qeydiyyatdan kecib. Zehmmet olmasa Google ile daxil olun.");

        var isPasswordValid = await userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
            return ResponseModel<string>.FailureResponse("Email ve ya sifre yanlisdir.");

        if (!user.EmailConfirmed)
            return ResponseModel<string>.FailureResponse("Zehmet olmasa evvelce emailinizi tesdiqleyin.");

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtTokenService.GenerateTokenAsync(user, roles);

        return ResponseModel<string>.SuccessResponse(token);
    }

    public async Task<ResponseModel<string>> GoogleLoginAsync(GoogleLoginDto dto)
    {
        var googleUser = await googleTokenValidator.ValidateAsync(dto.IdToken);
        if (googleUser == null)
            return ResponseModel<string>.FailureResponse("Google token etibarsizdir.");

        if (!googleUser.EmailVerified)
            return ResponseModel<string>.FailureResponse("Google hesabinizin emaili tesdiq olunmayib.");

        var existingUserByGoogleId = await userManager.Users
            .FirstOrDefaultAsync(u => u.GoogleId == googleUser.GoogleId);

        if (existingUserByGoogleId != null)
        {
            var roles = await userManager.GetRolesAsync(existingUserByGoogleId);
            var token = jwtTokenService.GenerateTokenAsync(existingUserByGoogleId, roles);
            return ResponseModel<string>.SuccessResponse(token);
        }

        var existingUserByEmail = await userManager.FindByEmailAsync(googleUser.Email);
        if (existingUserByEmail != null)
        {
            existingUserByEmail.GoogleId = googleUser.GoogleId;
            if (existingUserByEmail.AuthProvider == AuthProvider.Local)
            {
                existingUserByEmail.AuthProvider = AuthProvider.Google;
            }
            existingUserByEmail.EmailConfirmed = true;
            await userManager.UpdateAsync(existingUserByEmail);

            var roles = await userManager.GetRolesAsync(existingUserByEmail);
            var token = jwtTokenService.GenerateTokenAsync(existingUserByEmail, roles);
            return ResponseModel<string>.SuccessResponse(token);
        }

        var newUser = new AppUser(googleUser.FullName, googleUser.GoogleId, AuthProvider.Google)
        {
            UserName = googleUser.Email,
            Email = googleUser.Email,
            EmailConfirmed = true
        };

        var createResult = await userManager.CreateAsync(newUser);
        if (!createResult.Succeeded)
            return ResponseModel<string>.FailureResponse(createResult.Errors.Select(e => e.Description).ToList());

        await userManager.AddToRoleAsync(newUser, "User");

        var newUserRoles = await userManager.GetRolesAsync(newUser);
        var newUserToken = jwtTokenService.GenerateTokenAsync(newUser, newUserRoles);
        return ResponseModel<string>.SuccessResponse(newUserToken);
    }

    public async Task<ResponseModel<bool>> ForgotPasswordAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            return ResponseModel<bool>.FailureResponse("Istifadeci tapilmadi.");

        if (user.AuthProvider == AuthProvider.Google)
            return ResponseModel<bool>.FailureResponse("Google hesabi ucun sifre sifirlama mumkun deyil.");

        var token = await userManager.GeneratePasswordResetTokenAsync(user);

        byte[] tokenBytes = System.Text.Encoding.UTF8.GetBytes(token);
        var encodedToken = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(tokenBytes);
        var callbackUrl = $"https://localhost:7000/api/Account/ResetPassword?email={user.Email}&token={encodedToken}";
        string mailBody = $@"<h3>Sifre Sifirlama Isteyi</h3>
                             <p>Salam {user.FullName},</p>
                             <p>Sifrenizi yenilemek ucun asagidaki linke klikleyin:</p>
                             <a href='{callbackUrl}'>Sifreni Yenile</a>
                             <p>Eger bu isteyi siz etmemisinizse, bu emaili gormezden gelin.</p>";

        await mailService.SendEmailAsync(user.Email!, "Sifre Sifirlama", mailBody);

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return ResponseModel<bool>.FailureResponse("Istifadeci tapilmadi.");

        if (user.AuthProvider == AuthProvider.Google)
            return ResponseModel<bool>.FailureResponse("Google hesabi ucun sifre sifirlama mumkun deyil.");

        if (dto.NewPassword != dto.ConfirmPassword)
            return ResponseModel<bool>.FailureResponse("Yeni sifre ve tesdiq sifresi uygun gelmir.");

        var decodedTokenBytes = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlDecode(dto.Token);
        var orginalToken = System.Text.Encoding.UTF8.GetString(decodedTokenBytes);
        var result = await userManager.ResetPasswordAsync(user, orginalToken, dto.NewPassword);
        if (!result.Succeeded)
            return ResponseModel<bool>.FailureResponse(result.Errors.Select(e => e.Description).ToList());
        return ResponseModel<bool>.SuccessResponse(true);
    }
}
