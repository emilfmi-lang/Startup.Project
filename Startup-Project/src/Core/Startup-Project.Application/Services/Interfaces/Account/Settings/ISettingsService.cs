using Microsoft.AspNetCore.Http;
using Startup_Project.Application.Dtos.Account;
using Startup_Project.Application.Models;

namespace Startup_Project.Application.Services.Interfaces.Account.Settings;

public interface ISettingsService
{
    Task<ResponseModel<SettingsOverviewDto>> GetSettingsOverviewAsync();
    Task<ResponseModel<UserProfileDto>> GetProfileAsync();
    Task<ResponseModel<UserProfileDto>> UpdateProfileAsync(UpdateProfileDto dto);
    Task<ResponseModel<string>> ChangePhotoAsync(IFormFile photo);
    Task<ResponseModel<bool>> RemovePhotoAsync();
    Task<ResponseModel<bool>> ChangePasswordAsync(ChangePasswordDto dto);
    Task<ResponseModel<ActiveSessionsResponseDto>> GetActiveSessionsAsync();
    Task<ResponseModel<bool>> RevokeSessionAsync(Guid sessionId);
    Task<ResponseModel<bool>> RevokeAllOtherSessionsAsync();
    Task<ResponseModel<NotificationSettingsDto>> GetNotificationSettingsAsync();
    Task<ResponseModel<NotificationSettingsDto>> UpdateNotificationSettingsAsync(NotificationSettingsDto dto);
    Task<ResponseModel<UserPreferencesDto>> GetPreferencesAsync();
    Task<ResponseModel<UserPreferencesDto>> UpdatePreferencesAsync(UserPreferencesDto dto);
    Task<ResponseModel<bool>> DeleteAccountAsync();

    // Payment Methods
    Task<ResponseModel<PaymentMethodsResponseDto>> GetPaymentMethodsAsync();
    Task<ResponseModel<PaymentMethodDto>> AddPaymentMethodAsync(AddPaymentMethodDto dto);
    Task<ResponseModel<bool>> RemovePaymentMethodAsync(Guid methodId);
    Task<ResponseModel<bool>> SetDefaultPaymentMethodAsync(Guid methodId);


}
