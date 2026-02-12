using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Startup_Project.Application.Abstractions.Services;
using Startup_Project.Application.Abstractions.UnitOfWork;
using Startup_Project.Application.Dtos.Account;
using Startup_Project.Application.Dtos.Account.Password;
using Startup_Project.Application.Dtos.User;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces.Account;
using Startup_Project.Application.Validators.Account;
using Startup_Project.Domain.Abstractions.Repositories;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Exceptions;


namespace Startup_Project.Application.Services.Concretes.Account;

public class AccountService(
    UserManager<AppUser> userManager,
    IMapper mapper,
    IJwtTokenService jwtTokenService,
    IMailService mailService,
    IGoogleTokenValidator googleTokenValidator,
    IGoogleAuthService googleAuthService,
    IGenericRepository<SmsOtp> smsOtpRepository,
    IGenericRepository<EmailOtp> emailOtpRepository,
    ISmsService smsService,
    IUnitOfWork unitOfWork
    ) : IAccountService
{
    public async Task<ResponseModel<string>> VerifyAndLoginAsync(VerifyCodeDto dto)
    {
        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            throw NotFoundException.UserByEmail(dto.Email);
        if (user.VerificationCode != dto.Code)
            throw BusinessException.InvalidVerificationCode();
        if (user.CodeExpireDate < DateTime.UtcNow)
            throw BusinessException.VerificationCodeExpired();
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
            throw BusinessException.ValidationFailed(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var user = mapper.Map<AppUser>(dto);
        user.UserName = dto.Email;
        user.AuthProvider = AuthProvider.Local;

        user.VerificationCode = new Random().Next(1000, 9999).ToString();
        user.CodeExpireDate = DateTime.UtcNow.AddMinutes(5);

        var result = await userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            throw BusinessException.ValidationFailed(string.Join(", ", result.Errors.Select(e => e.Description)));

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
            throw NotFoundException.UserByEmail(dto.Email);

        if (user.AuthProvider == AuthProvider.Google)
            throw BusinessException.GoogleAuthRequired();

        if (user.AuthProvider == AuthProvider.Phone)
            throw BusinessException.PhoneAuthRequired();

        var isPasswordValid = await userManager.CheckPasswordAsync(user, dto.Password);
        if (!isPasswordValid)
            throw BusinessException.InvalidCredentials();

        if (!user.EmailConfirmed)
            throw BusinessException.EmailNotConfirmed();

        var roles = await userManager.GetRolesAsync(user);
        var token = jwtTokenService.GenerateTokenAsync(user, roles);

        return ResponseModel<string>.SuccessResponse(token);
    }

    public async Task<ResponseModel<string>> GoogleLoginAsync(GoogleLoginDto dto)
    {
        // Yeni IGoogleAuthService istifadə et
        return await googleAuthService.GoogleLoginAsync(dto.IdToken);
    }

    public async Task<ResponseModel<bool>> ForgotPasswordAsync(string email)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
            throw NotFoundException.UserByEmail(email);

        if (user.AuthProvider == AuthProvider.Google)
            throw BusinessException.PasswordResetNotAllowed("Google");

        if (user.AuthProvider == AuthProvider.Phone)
            throw BusinessException.PasswordResetNotAllowed("Telefon");

        // Deactivate previous OTP codes
        var activeCodes = await emailOtpRepository.Table
            .Where(x => x.Email == email && !x.IsUsed && x.ExpiryDate > DateTime.UtcNow)
            .ToListAsync();

        foreach (var code in activeCodes)
        {
            code.MarkAsUsed();
        }

        // Generate new OTP code
        var random = new Random();
        var otpCode = random.Next(100000, 999999).ToString();

        // Create and save OTP record
        var emailOtp = new EmailOtp(email, otpCode, expiryMinutes: 5);
        await emailOtpRepository.AddAsync(emailOtp);
        await unitOfWork.SaveChangesAsync();

        // Send email with OTP code
        string mailBody = $@"<h3>Şifrə Sıfırlama</h3>
                             <p>Salam {user.FullName},</p>
                             <p>Şifrənizi sıfırlamaq üçün aşağıdakı kodu istifadə edin:</p>
                             <h2 style='color: #007bff; text-align: center;'>{otpCode}</h2>
                             <p>Bu kod 5 dəqiqə ərzində etibarlıdır.</p>
                             <p>Eger bu isteyi siz etmemisinizse, bu emaili gormezden gelin.</p>";

        await mailService.SendEmailAsync(user.Email!, "Şifrə Sıfırlama Kodu", mailBody);

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<bool>> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var validator = new ResetPasswordDtoValidator();
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw BusinessException.ValidationFailed(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var user = await userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            throw NotFoundException.UserByEmail(dto.Email);

        if (user.AuthProvider == AuthProvider.Google)
            throw BusinessException.PasswordResetNotAllowed("Google");

        if (user.AuthProvider == AuthProvider.Phone)
            throw BusinessException.PasswordResetNotAllowed("Telefon");

        // Verify Email OTP
        var otp = await emailOtpRepository.Table
            .Where(x => x.Email == dto.Email && !x.IsUsed && x.ExpiryDate > DateTime.UtcNow && x.AttemptCount < 3)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (otp == null)
            throw BusinessException.EmailOtpNotFound();

        if (otp.Code != dto.Code)
        {
            otp.IncrementAttempt();

            if (otp.AttemptCount >= 3)
            {
                otp.MarkAsUsed();
                await unitOfWork.SaveChangesAsync();
                throw BusinessException.EmailOtpMaxAttemptsExceeded();
            }

            await unitOfWork.SaveChangesAsync();
            throw BusinessException.EmailOtpInvalidCode(3 - otp.AttemptCount);
        }

        // Mark OTP as used
        otp.MarkAsUsed();
        await unitOfWork.SaveChangesAsync();

        // Change password using UserManager
        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var result = await userManager.ResetPasswordAsync(user, token, dto.NewPassword);

        if (!result.Succeeded)
            throw BusinessException.ValidationFailed(string.Join(", ", result.Errors.Select(e => e.Description)));

        return ResponseModel<bool>.SuccessResponse(true);
    }

    #region SMS OTP Methods

    public async Task<ResponseModel<bool>> SendSmsOtpAsync(SendSmsOtpDto dto)
    {
        var validator = new SendSmsOtpDtoValidator();
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw BusinessException.ValidationFailed(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var activeCodes = await smsOtpRepository.Table
            .Where(x => x.PhoneNumber == dto.PhoneNumber && !x.IsUsed && x.ExpiryDate > DateTime.UtcNow)
            .ToListAsync();

        foreach (var code in activeCodes)
        {
            code.MarkAsUsed();
        }

        var random = new Random();
        var otpCode = random.Next(100000, 999999).ToString();

        // Create new OTP record
        var smsOtp = new SmsOtp(dto.PhoneNumber, otpCode, expiryMinutes: 3);
        await smsOtpRepository.AddAsync(smsOtp);
        await unitOfWork.SaveChangesAsync();

        // Send SMS
        var message = $"Doğrulama kodunuz: {otpCode}. Bu kod 3 dəqiqə ərzində etibarlıdır.";
        await smsService.SendAsync(dto.PhoneNumber, message);

        return ResponseModel<bool>.SuccessResponse(true);
    }

    public async Task<ResponseModel<string>> VerifySmsOtpAsync(VerifySmsOtpDto dto)
    {
        var validator = new VerifySmsOtpDtoValidator();
        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
            throw BusinessException.ValidationFailed(string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage)));

        var otp = await smsOtpRepository.Table
            .Where(x => x.PhoneNumber == dto.PhoneNumber && !x.IsUsed && x.ExpiryDate > DateTime.UtcNow && x.AttemptCount < 3)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (otp == null)
            throw BusinessException.OtpNotFound();

        if (otp.Code != dto.Code)
        {
            otp.IncrementAttempt();

            if (otp.AttemptCount >= 3)
            {
                otp.MarkAsUsed();
                await unitOfWork.SaveChangesAsync();
                throw BusinessException.OtpMaxAttemptsExceeded();
            }

            await unitOfWork.SaveChangesAsync();
            throw BusinessException.OtpInvalidCode(3 - otp.AttemptCount);
        }

        otp.MarkAsUsed();
        await unitOfWork.SaveChangesAsync();

        var user = await userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber);

        if (user == null)
        {
            var phoneDigits = dto.PhoneNumber.Replace("+", "");
            var placeholderEmail = $"{phoneDigits}@phone.local";

            user = new AppUser
            {
                UserName = dto.PhoneNumber,
                Email = placeholderEmail,
                PhoneNumber = dto.PhoneNumber,
                PhoneNumberConfirmed = true,
                EmailConfirmed = false, 
                FullName = $"User_{dto.PhoneNumber[^4..]}", 
                AuthProvider = AuthProvider.Phone
            };

            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded)
                throw BusinessException.ValidationFailed(string.Join(", ", createResult.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(user, "User");
        }
        else
        {
            if (!user.PhoneNumberConfirmed)
            {
                user.PhoneNumberConfirmed = true;
                await userManager.UpdateAsync(user);
            }
        }

                var roles = await userManager.GetRolesAsync(user);
                var token = jwtTokenService.GenerateTokenAsync(user, roles);

                return ResponseModel<string>.SuccessResponse(token);
            }

            #endregion

            #region Email OTP Methods (for Password Reset)

            public async Task<ResponseModel<bool>> SendPasswordResetOtpAsync(string email)
            {
                return await ForgotPasswordAsync(email);
            }

            public async Task<ResponseModel<bool>> VerifyPasswordResetOtpAsync(ResetPasswordDto dto)
            {
                return await ResetPasswordAsync(dto);
            }

            #endregion
        }
