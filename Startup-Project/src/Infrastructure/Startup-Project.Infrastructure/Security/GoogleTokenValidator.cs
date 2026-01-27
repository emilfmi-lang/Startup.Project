using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using Startup_Project.Application.Abstractions.Services;

namespace Startup_Project.Infrastructure.Security;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    private readonly string _clientId;

    public GoogleTokenValidator(IConfiguration configuration)
    {
        _clientId = configuration["GoogleAuth:ClientId"] 
            ?? throw new ArgumentNullException("GoogleAuth:ClientId configuration is missing");
    }

    public async Task<GoogleUserInfo?> ValidateAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _clientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

            if (payload == null)
                return null;

            return new GoogleUserInfo
            {
                GoogleId = payload.Subject,
                Email = payload.Email,
                FullName = payload.Name ?? payload.Email.Split('@')[0],
                PictureUrl = payload.Picture,
                EmailVerified = payload.EmailVerified
            };
        }
        catch (InvalidJwtException)
        {
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }
}
