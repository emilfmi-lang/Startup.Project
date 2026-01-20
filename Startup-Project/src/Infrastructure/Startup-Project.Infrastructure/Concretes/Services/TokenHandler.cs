using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Startup_Project.Application.Abstractions.Services;
using Startup_Project.Application.Dtos.Token;
using Startup_Project.Domain.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Startup_Project.Infrastructure.Concretes.Services;

public class TokenHandler(IConfiguration configuration) : ITokenHandler
{
    public async Task<TokenDto> CreateAccessToken(User user)
    {
        TokenDto tokenDto = new();
        var jwtSettings = configuration.GetSection("JwtSettings");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("FullName", user.FullName ?? "")
        };

        DateTime expires = DateTime.UtcNow.AddMinutes(60);

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expires,
            signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
        );
        tokenDto.AccessToken = new JwtSecurityTokenHandler().WriteToken(token);
        tokenDto.Expiration = expires;
        return tokenDto;
    }
}
