namespace Startup_Project.Domain.Enums.Errors;

public enum BusinessErrorCodes
{
    // GENERAL ERRORS (1000-1999)
    UnknownError = 1000,
    ValidationFailed = 1001,
    UnauthorizedAccess = 1002,
    ForbiddenAccess = 1003,

    // USER ERRORS (2000-2999)
    UserNotFound = 2000,
    UserAlreadyExists = 2001,
    InvalidCredentials = 2002,
    EmailNotConfirmed = 2003,
    PhoneNotConfirmed = 2004,
    AccountLocked = 2005,
    InvalidVerificationCode = 2006,
    VerificationCodeExpired = 2007,

    // AUTH PROVIDER ERRORS (2100-2199)
    GoogleTokenInvalid = 2100,
    GoogleEmailNotVerified = 2101,
    GoogleAuthRequired = 2102,
    PhoneAuthRequired = 2103,
    PasswordResetNotAllowed = 2104,

    // OTP ERRORS (2200-2299)
    OtpNotFound = 2200,
    OtpExpired = 2201,
    OtpInvalidCode = 2202,
    OtpMaxAttemptsExceeded = 2203,

    // EMAIL OTP ERRORS (2250-2299)
    EmailOtpNotFound = 2250,
    EmailOtpExpired = 2251,
    EmailOtpInvalidCode = 2252,
    EmailOtpMaxAttemptsExceeded = 2253,

    // GYM ERRORS (3000-3999)
    GymNotFound = 3000,
    GymAlreadyExists = 3001,
    GymClosed = 3002,
    InvalidGymOwner = 3003,

    // PACKAGE ERRORS (4000-4999)
    PackageNotFound = 4000,
    PackageExpired = 4001,
    PackageAlreadyPurchased = 4002,

    // PROMO CODE ERRORS (5000-5999)
    PromoCodeNotFound = 5000,
    PromoCodeExpired = 5001,
    PromoCodeDeactivated = 5002,
    PromoCodeUsageLimitExceeded = 5003,
    PromoCodeAlreadyUsed = 5004,
    PromoCodeNotApplicable = 5005,
    PromoCodeMinimumAmountNotMet = 5006,
    PromoCodeConcurrencyError = 5007,

    // PAYMENT ERRORS (6000-6999)
    PaymentNotFound = 6000,
    PaymentFailed = 6001,
    PaymentAlreadyProcessed = 6002,
    InsufficientFunds = 6003,

    // SUBSCRIPTION ERRORS (7000-7999)
    SubscriptionNotFound = 7000,
    SubscriptionExpired = 7001,
    SubscriptionAlreadyActive = 7002,

    // FAVORITE ERRORS (8000-8999)
    FavoriteNotFound = 8000,
    AlreadyInFavorites = 8001,

    // TRAINER ERRORS (9000-9999)
    TrainerNotFound = 9000,
    TrainerNotAvailable = 9001,
    SessionNotFound = 9002,
    SessionAlreadyBooked = 9003,
    TrainerAlreadyExists = 9004,
    TrainerNotVerified = 9005,
    SessionNotPaid = 9006,
    SessionExpired = 9007,
    SessionAlreadyUsed = 9008,
    SessionAlreadyCancelled = 9009,
    SessionAlreadyProcessed = 9010,

    // CHECK-IN ERRORS (10000-10999)
    CheckInTokenInvalid = 10000,
    CheckInTokenExpired = 10001,
    AlreadyCheckedIn = 10002,
    CheckInNotAllowed = 10003,

    // REVIEW ERRORS (11000-11999)
    ReviewNotFound = 11000,
    AlreadyReviewed = 11001,

    // CAMPAIGN ERRORS (12000-12999)
    CampaignNotFound = 12000,
    CampaignExpired = 12001,
    CampaignDeactivated = 12002,
    CampaignUsageLimitExceeded = 12003,
    CampaignNotApplicable = 12004,
    CampaignMinimumAmountNotMet = 12005,
    CampaignRuleNotFound = 12006,
    CampaignRuleEvaluationFailed = 12007,
    CampaignAlreadyExists = 12008,

    // PRICING ERRORS (13000-13999)
    PricingCalculationFailed = 13000,
    InvalidPricingContext = 13001,
    NoCampaignsApplicable = 13002,
    // SETTINGS ERRORS (14000-14999)
    CurrentPasswordIncorrect = 14000,
    NewPasswordSameAsOld = 14001,
    PasswordTooWeak = 14002,
    SessionNotFoundSetting = 14003,
    CannotRevokeCurrentSession = 14004,
    InvalidPhotoFormat = 14005,
    PhotoSizeTooLarge = 14006,
    AccountDeletionFailed = 14007,
    ProfileUpdateFailed = 14008,

    // PAYMENT METHOD ERRORS (15000-15999)
    PaymentMethodNotFound = 15000,
    PaymentMethodLimitExceeded = 15001,
    PaymentMethodExpired = 15002,
    PaymentMethodAlreadyExists = 15003,
        CannotRemoveDefaultPaymentMethod = 15004,

            // SCHEDULED SESSION ERRORS (16000-16999)
            ScheduledSessionNotFound = 16000,
            SessionTimeCollision = 16001,
            MemberSubscriptionExpired = 16002,
            ScheduledSessionInvalidStatus = 16003,

                // TRAINER STUDENT ERRORS (17000-17999)
                StudentAlreadyAssigned = 17000,
                StudentNotFound = 17001,

                // SESSION PAYMENT ERRORS (18000-18999)
                SessionPaymentNotFound = 18000,
                SessionPaymentAlreadyPaid = 18001,
                SessionPaymentFailed = 18002,


            }
