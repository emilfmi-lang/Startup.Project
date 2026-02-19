using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Startup_Project.Application.Abstractions.Services;
using Startup_Project.Application.Dtos.Account;
using Startup_Project.Application.Dtos.Token;
using Startup_Project.Application.Interfaces;
using Startup_Project.Application.Models;
using Startup_Project.Application.Services.Interfaces.Account;
using Startup_Project.Domain.Entities;
using Startup_Project.Domain.Entities.Identity;
using Startup_Project.Domain.Enums;
using Startup_Project.Domain.Exceptions;

namespace Startup_Project.Infrastructure.Concretes.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<AppUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<GoogleAuthService> _logger;
    private readonly IApplicationDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public GoogleAuthService(
        IConfiguration configuration,
        UserManager<AppUser> userManager,
        IJwtTokenService jwtTokenService,
        ILogger<GoogleAuthService> logger,
        IApplicationDbContext context,
        IHttpContextAccessor httpContextAccessor)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
        _jwtTokenService = jwtTokenService ?? throw new ArgumentNullException(nameof(jwtTokenService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public async Task<ResponseModel<TokenDto>> GoogleLoginAsync(string idToken)
    {
        try
        {
            _logger.LogInformation("Google login basladi...");

            var googleClientId = _configuration["GoogleAuth:ClientId"];
            if (string.IsNullOrWhiteSpace(googleClientId))
                throw BusinessException.Custom(
                    Domain.Enums.Errors.BusinessErrorCodes.GoogleTokenInvalid,
                    "Google konfiqurasiyonu xetasidir.");

            var googleTokenValidation = await ValidateGoogleTokenAsync(idToken, googleClientId);

            if (!googleTokenValidation.IsValid)
                throw BusinessException.GoogleTokenInvalid();

            if (!googleTokenValidation.EmailVerified)
                throw BusinessException.GoogleEmailNotVerified();

            // Google ID ile user axtar
            var user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.GoogleId == googleTokenValidation.GoogleId);

            if (user is null)
            {
                // Email ile axtar
                user = await _userManager.FindByEmailAsync(googleTokenValidation.Email);
                if (user is not null)
                {
                    user.GoogleId = googleTokenValidation.GoogleId;
                    if (user.AuthProvider == AuthProvider.Local)
                        user.AuthProvider = AuthProvider.Google;
                    user.EmailConfirmed = true;
                    await _userManager.UpdateAsync(user);
                }
                else
                {
                    // Yeni user yarat
                    user = new AppUser(
                        googleTokenValidation.FullName,
                        googleTokenValidation.GoogleId,
                        AuthProvider.Google)
                    {
                        UserName = googleTokenValidation.Email,
                        Email = googleTokenValidation.Email,
                        EmailConfirmed = true,
                        ProfileImageUrl = googleTokenValidation.PictureUrl
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                        throw BusinessException.ValidationFailed(
                            string.Join(", ", createResult.Errors.Select(e => e.Description)));

                    await _userManager.AddToRoleAsync(user, "User");
                }
            }

            return await CreateTokenResponseAsync(user);
        }
        catch (BusinessException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google login xetasi");
            throw BusinessException.Custom(
                Domain.Enums.Errors.BusinessErrorCodes.UnknownError,
                "Google login prosesinde xeta bas verdi.");
        }
    }

    private async Task<ResponseModel<TokenDto>> CreateTokenResponseAsync(AppUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.GenerateAccessToken(user, roles);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();

        var refreshDays = int.TryParse(_configuration["JwtSettings:RefreshTokenExpiryDays"], out var d) ? d : 14;
        var expiryMinutes = int.TryParse(_configuration["JwtSettings:AccessTokenExpiryMinutes"], out var m) ? m : 15;

        var httpContext = _httpContextAccessor.HttpContext;
        var userAgent = httpContext?.Request.Headers.UserAgent.ToString() ?? "Unknown";
        var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();
        var deviceInfo = userAgent.Length > 200 ? userAgent[..200] : userAgent;

        var oldSessions = await _context.UserSessions
            .Where(s => s.UserId == user.Id && s.IsCurrent && !s.IsDeleted)
            .ToListAsync();
        foreach (var old in oldSessions)
            old.UnmarkAsCurrent();

        var session = new UserSession(
            userId: user.Id,
            deviceInfo: deviceInfo,
            refreshToken: refreshToken,
            refreshTokenExpiry: DateTime.UtcNow.AddDays(refreshDays),
            ipAddress: ipAddress,
            userAgent: userAgent.Length > 500 ? userAgent[..500] : userAgent);
        session.MarkAsCurrent();

        _context.UserSessions.Add(session);
        await _context.SaveChangesAsync();

        return ResponseModel<TokenDto>.SuccessResponse(new TokenDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Expiration = DateTime.UtcNow.AddMinutes(expiryMinutes)
        });
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
