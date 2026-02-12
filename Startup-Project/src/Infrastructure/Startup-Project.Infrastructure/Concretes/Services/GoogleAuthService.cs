using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Startup_Project.Application.Abstractions.Services;
using Startup_Project.Application.Dtos.Account;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces.Account;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Exceptions;

namespace Startup_Project.Infrastructure.Concretes.Services;

/// <summary>
/// Google OAuth2 autentifikasiya servisi
/// Google ID Token-ini do?rulay?b JWT token qaytar?r
/// </summary>
public class GoogleAuthService : IGoogleAuthService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(
        IConfiguration configuration,
        UserManager<AppUser> userManager,
        IJwtTokenService jwtTokenService,
        ILogger<GoogleAuthService> logger)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Google ID Token-i do?rulay?b, JWT token qaytar?r
    /// </summary>
    public async Task<ResponseModel<string>> GoogleLoginAsync(string idToken)
    {
        try
        {
            _logger.LogInformation("Google login ba?lad?. Token do?rulanmas? ba?lan?r...");

            // Google ClientId-ni konfigurasyondan oxu
            var googleClientId = _configuration["GoogleAuth:ClientId"];
            if (string.IsNullOrWhiteSpace(googleClientId))
            {
                _logger.LogError("Google ClientId appsettings.json-da tap?lmad?");
                throw BusinessException.Custom(
                    Domain.Enums.Errors.BusinessErrorCodes.GoogleTokenInvalid,
                    "Google konfigurasyonu x?tas?d?r.");
            }

            // Google token-i do?rula
            var googleTokenValidation = await ValidateGoogleTokenAsync(idToken, googleClientId);

            if (!googleTokenValidation.IsValid)
            {
                _logger.LogWarning($"Google token validasiya x?tas?: {googleTokenValidation.ErrorMessage}");
                throw BusinessException.GoogleTokenInvalid();
            }

            if (!googleTokenValidation.EmailVerified)
            {
                _logger.LogWarning($"Google email t?sdiql?nm?yib: {googleTokenValidation.Email}");
                throw BusinessException.GoogleEmailNotVerified();
            }

            _logger.LogInformation($"Google token do?ruland?. Email: {googleTokenValidation.Email}");

            // Bazada bu Google ID il? user axtar???
            var existingUserByGoogleId = await _userManager.Users
                .FirstOrDefaultAsync(u => u.GoogleId == googleTokenValidation.GoogleId);

            // ?g?r Google ID il? user varsa, d?rhal JWT token qaytar?
            if (existingUserByGoogleId != null)
            {
                _logger.LogInformation($"Google ID il? user tap?ld?: {existingUserByGoogleId.Email}");
                var roles = await _userManager.GetRolesAsync(existingUserByGoogleId);
                var token = _jwtTokenService.GenerateTokenAsync(existingUserByGoogleId, roles);
                return ResponseModel<string>.SuccessResponse(token);
            }

            // ?g?r email il? user varsa, Google ID-ni ?lav? et
            var existingUserByEmail = await _userManager.FindByEmailAsync(googleTokenValidation.Email);
            if (existingUserByEmail != null)
            {
                _logger.LogInformation($"Email il? user tap?ld?, Google ID ?lav? edilir: {googleTokenValidation.Email}");

                existingUserByEmail.GoogleId = googleTokenValidation.GoogleId;

                // ?g?r user Local auth il? qeydiyyat olubsa, auth provider-i d?yi?dir
                if (existingUserByEmail.AuthProvider == AuthProvider.Local)
                {
                    existingUserByEmail.AuthProvider = AuthProvider.Google;
                }

                existingUserByEmail.EmailConfirmed = true;
                
                var updateResult = await _userManager.UpdateAsync(existingUserByEmail);
                if (!updateResult.Succeeded)
                {
                    _logger.LogError($"User update x?tas?: {string.Join(", ", updateResult.Errors.Select(e => e.Description))}");
                    throw BusinessException.ValidationFailed(
                        string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                }

                var roles = await _userManager.GetRolesAsync(existingUserByEmail);
                var token = _jwtTokenService.GenerateTokenAsync(existingUserByEmail, roles);
                return ResponseModel<string>.SuccessResponse(token);
            }

            // Yeni user yaratma
            _logger.LogInformation($"Yeni Google user yarad?l?r: {googleTokenValidation.Email}");

            var newUser = new AppUser(
                googleTokenValidation.FullName,
                googleTokenValidation.GoogleId,
                AuthProvider.Google)
            {
                UserName = googleTokenValidation.Email,
                Email = googleTokenValidation.Email,
                EmailConfirmed = true,
                ProfileImageUrl = googleTokenValidation.PictureUrl
            };

            var createResult = await _userManager.CreateAsync(newUser);
            if (!createResult.Succeeded)
            {
                _logger.LogError($"User yaratma x?tas?: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
                throw BusinessException.ValidationFailed(
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
            }

            // Default "User" rolu ?lav? et
            var roleResult = await _userManager.AddToRoleAsync(newUser, "User");
            if (!roleResult.Succeeded)
            {
                _logger.LogError($"Role ?lav? etm? x?tas?: {string.Join(", ", roleResult.Errors.Select(e => e.Description))}");
                throw BusinessException.ValidationFailed(
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));
            }

            _logger.LogInformation($"Yeni Google user u?urla yarad?ld?: {newUser.Email}");

            // JWT token gener? et
            var newUserRoles = await _userManager.GetRolesAsync(newUser);
            var newUserToken = _jwtTokenService.GenerateTokenAsync(newUser, newUserRoles);

            return ResponseModel<string>.SuccessResponse(newUserToken);
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Google login s?hvi: {ex.Message}", ex);
            throw BusinessException.Custom(
                Domain.Enums.Errors.BusinessErrorCodes.UnknownError,
                "Google login prosesind? x?ta ba? verdi.");
        }
    }

    /// <summary>
    /// Google token-i do?rula v? m?lumat ç?xar
    /// </summary>
    private async Task<GoogleTokenValidationResult> ValidateGoogleTokenAsync(
        string idToken,
        string googleClientId)
    {
        try
        {
            _logger.LogInformation("Google token do?rulanmas? ba?lan?r...");
            _logger.LogDebug($"ClientId: {googleClientId}");
            _logger.LogDebug($"Token (first 50 chars): {idToken?.Substring(0, Math.Min(50, idToken?.Length ?? 0))}...");

            if (string.IsNullOrWhiteSpace(idToken))
            {
                _logger.LogError("IdToken bo? v? ya null-d?r");
                return new GoogleTokenValidationResult
                {
                    IsValid = false,
                    ErrorMessage = "IdToken bo? olmamal?d?r."
                };
            }

            // Google token-i do?rula - ValidationSettings il?
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { googleClientId }
                });

            _logger.LogInformation($"Google token do?ruland?. Subject: {payload.Subject}");
            _logger.LogDebug($"Token Audience: {payload.Audience}");
            _logger.LogDebug($"Token Email: {payload.Email}");
            _logger.LogDebug($"Token Name: {payload.Name}");
            _logger.LogDebug($"Token EmailVerified: {payload.EmailVerified}");

            // Email-in do?rulan?b-do?rulanmad???n? yoxla
            var emailVerified = payload.EmailVerified;

            return new GoogleTokenValidationResult
            {
                IsValid = true,
                GoogleId = payload.Subject,
                Email = payload.Email,
                FullName = payload.Name ?? "Google User",
                PictureUrl = payload.Picture,
                EmailVerified = emailVerified
            };
        }
        catch (InvalidOperationException iex)
        {
            _logger.LogWarning($"InvalidOperationException - Token validasiya x?tas?: {iex.Message}");
            _logger.LogWarning($"Stack Trace: {iex.StackTrace}");
            return new GoogleTokenValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Token validasiyas? u?ursuz oldu: {iex.Message}"
            };
        }
        catch (ArgumentException aex)
        {
            _logger.LogWarning($"ArgumentException - Parametr x?tas?: {aex.Message}");
            _logger.LogWarning($"Stack Trace: {aex.StackTrace}");
            return new GoogleTokenValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Parametr x?tas?: {aex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Gözl?nilm?z x?ta: {ex.GetType().Name} - {ex.Message}");
            _logger.LogError($"Stack Trace: {ex.StackTrace}");
            return new GoogleTokenValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Token do?rulanmas? zaman? x?ta ba? verdi: {ex.Message}"
            };
        }
    }
}
