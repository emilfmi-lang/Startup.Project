namespace Startup_Project.Domain.Enums.Notifications;

public enum NotificationType
{
    // Subscription
    SubscriptionCreated = 100,
    SubscriptionExpiringSoon = 101,
    SubscriptionExpired = 102,

    // Payment
    PaymentSuccessful = 200,
    PaymentFailed = 201,

    // Campaign & Promo
    NewCampaignAvailable = 300,
    PromoCodeReceived = 301,

    // Training Session
    SessionBooked = 400,
    SessionReminder = 401,
    SessionCancelled = 402,

    // Check-in
    CheckInSuccessful = 500,

    // System
    SystemAnnouncement = 900,
    WelcomeMessage = 901
}
