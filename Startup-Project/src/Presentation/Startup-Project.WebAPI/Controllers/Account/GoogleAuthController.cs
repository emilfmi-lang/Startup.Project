using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Account;
using Startup_Project.Application.Services.Interfaces.Account;

namespace Startup_Project.WebAPI.Controllers.Account;

/// <summary>
/// Google OAuth2 autentifikasiya endpoint-l?ri
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class GoogleAuthController(IGoogleAuthService googleAuthService) : ControllerBase
{
    /// <summary>
    /// Google ID Token il? login edin v? JWT token ?ld? edin
    /// </summary>
    /// <param name="request">Google ID Token</param>
    /// <returns>JWT Access Token</returns>
    /// <remarks>
    /// Misal request:
    /// 
    ///     POST /api/googleauth/login
    ///     {
    ///       "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6ImZmZjEzODYzNjY1NjI0OWZhZDhjYzhjNzE4NWQyZTFlODhlNTI2YTQiLCJ0eXAiOiJKV1QifQ..."
    ///     }
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] GoogleLoginDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await googleAuthService.GoogleLoginAsync(request.IdToken);
        return Ok(result);
    }
}
