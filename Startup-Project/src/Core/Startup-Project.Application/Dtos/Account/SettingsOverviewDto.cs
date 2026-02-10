namespace Startup_Project.Application.Dtos.Account;

public class SettingsOverviewDto
{
    public UserProfileDto Profile { get; set; } = null!;
    public NotificationSettingsDto Notifications { get; set; } = null!;
    public UserPreferencesDto Preferences { get; set; } = null!;
    public int ActiveSessionsCount { get; set; }
    public int PaymentMethodsCount { get; set; }
}
