namespace Startup_Project.Application.Dtos.Account;

public class NotificationSettingsDto
{
    public bool PushNotifications { get; set; }
    public bool EmailNotifications { get; set; }
    public bool SmsNotifications { get; set; }

    public bool WorkoutReminders { get; set; }
    public bool PromotionalOffers { get; set; }
    public bool SessionUpdates { get; set; }
    public bool NewFeatures { get; set; }
}
