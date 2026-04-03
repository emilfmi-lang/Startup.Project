using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startup_Project.Application.Dtos.Account;
using Startup_Project.Application.Services.Interfaces.Account.Settings;

namespace Startup_Project.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SettingsController(ISettingsService settingsService) : ControllerBase
{
    #region -------------------- OVERVIEW --------------------

    [HttpGet]
    public async Task<IActionResult> GetOverview()
        => Ok(await settingsService.GetSettingsOverviewAsync());

    #endregion

    #region -------------------- PROFILE --------------------

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
        => Ok(await settingsService.GetProfileAsync());

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto dto)
        => Ok(await settingsService.UpdateProfileAsync(dto));

    [HttpPost("profile/photo")]
    public async Task<IActionResult> ChangePhoto(IFormFile photo)
        => Ok(await settingsService.ChangePhotoAsync(photo));

    [HttpDelete("profile/photo")]
    public async Task<IActionResult> RemovePhoto()
        => Ok(await settingsService.RemovePhotoAsync());

    #endregion

    #region -------------------- SECURITY --------------------

    /// <summary>
    /// ?ifr? d?yi?
    /// </summary>
    [HttpPut("security/password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        => Ok(await settingsService.ChangePasswordAsync(dto));

    /// <summary>
    /// Aktiv sessiyalar
    /// </summary>
    [HttpGet("security/sessions")]
    public async Task<IActionResult> GetActiveSessions()
        => Ok(await settingsService.GetActiveSessionsAsync());

    /// <summary>
    /// Sessiya revoke et
    /// </summary>
    [HttpDelete("security/sessions/{id:guid}")]
    public async Task<IActionResult> RevokeSession(Guid id)
        => Ok(await settingsService.RevokeSessionAsync(id));

    /// <summary>
    /// Bütün dig?r sessiyalar? revoke et
    /// </summary>
    [HttpDelete("security/sessions")]
    public async Task<IActionResult> RevokeAllOtherSessions()
        => Ok(await settingsService.RevokeAllOtherSessionsAsync());

    #endregion

    #region -------------------- NOTIFICATIONS --------------------

    /// <summary>
    /// Bildiri? ayarlar?
    /// </summary>
    [HttpGet("notifications")]
    public async Task<IActionResult> GetNotificationSettings()
        => Ok(await settingsService.GetNotificationSettingsAsync());

    /// <summary>
    /// Bildiri? ayarlar?n? yenil?
    /// </summary>
    [HttpPut("notifications")]
    public async Task<IActionResult> UpdateNotificationSettings([FromBody] NotificationSettingsDto dto)
        => Ok(await settingsService.UpdateNotificationSettingsAsync(dto));

    #endregion

    #region -------------------- PREFERENCES --------------------

    /// <summary>
    /// Preferences
    /// </summary>
    [HttpGet("preferences")]
    public async Task<IActionResult> GetPreferences()
        => Ok(await settingsService.GetPreferencesAsync());

    /// <summary>
    /// Preferences yenil?
    /// </summary>
    [HttpPut("preferences")]
    public async Task<IActionResult> UpdatePreferences([FromBody] UserPreferencesDto dto)
        => Ok(await settingsService.UpdatePreferencesAsync(dto));

    #endregion

    #region -------------------- PAYMENT METHODS --------------------

    /// <summary>
    /// Öd?ni? metodlar? siyah?s?
    /// </summary>
    [HttpGet("payment-methods")]
    public async Task<IActionResult> GetPaymentMethods()
        => Ok(await settingsService.GetPaymentMethodsAsync());

    /// <summary>
    /// Yeni öd?ni? metodu ?lav? et
    /// </summary>
    [HttpPost("payment-methods")]
    public async Task<IActionResult> AddPaymentMethod([FromBody] AddPaymentMethodDto dto)
        => Ok(await settingsService.AddPaymentMethodAsync(dto));

    /// <summary>
    /// Öd?ni? metodunu sil
    /// </summary>
    [HttpDelete("payment-methods/{id:guid}")]
    public async Task<IActionResult> RemovePaymentMethod(Guid id)
        => Ok(await settingsService.RemovePaymentMethodAsync(id));

    /// <summary>
    /// Default öd?ni? metodu t?yin et
    /// </summary>
    [HttpPut("payment-methods/{id:guid}/default")]
    public async Task<IActionResult> SetDefaultPaymentMethod(Guid id)
        => Ok(await settingsService.SetDefaultPaymentMethodAsync(id));

    #endregion

    #region -------------------- DANGER ZONE --------------------

    /// <summary>
    /// Hesab? sil (soft delete)
    /// </summary>
    [HttpDelete("account")]
    public async Task<IActionResult> DeleteAccount()
        => Ok(await settingsService.DeleteAccountAsync());

    #endregion
}
