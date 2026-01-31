using Startup_Project.Domain.Enums;

namespace Startup_Project.Domain.Exceptions;


public class BusinessException : BaseException
{
    public override int StatusCode => 400;
    public BusinessException(BusinessErrorCodes errorCode, string message)
        : base(errorCode, message)
    {
    }
    public BusinessException(BusinessErrorCodes errorCode, string message, Exception innerException)
        : base(errorCode, message, innerException)
    {
    }
    public static BusinessException PromoCodeExpired(string code, DateTime endDate)
        => new(BusinessErrorCodes.PromoCodeExpired,
               $"'{code}' promokodunun müddeti bitib. Son tarix: {endDate:dd.MM.yyyy}");

    public static BusinessException PromoCodeDeactivated(string code)
        => new(BusinessErrorCodes.PromoCodeDeactivated,
               $"'{code}' promokodu deaktiv edilib.");
    public static BusinessException PromoCodeUsageLimitExceeded(string code, int limit)
        => new(BusinessErrorCodes.PromoCodeUsageLimitExceeded,
               $"'{code}' promokodunun istifad? limiti ({limit}) dolub.");
    public static BusinessException PromoCodeAlreadyUsed(string code)
        => new(BusinessErrorCodes.PromoCodeAlreadyUsed,
               "Bu promokodu artiq istifade etmisiniz.");
    public static BusinessException PromoCodeNotApplicable(string code)
        => new(BusinessErrorCodes.PromoCodeNotApplicable,
               $"'{code}' promokodu seçilen pakete aid deyil.");

    public static BusinessException PromoCodeMinimumAmountNotMet(string code, decimal minAmount, decimal actualAmount)
        => new(BusinessErrorCodes.PromoCodeMinimumAmountNotMet,
               $"'{code}' promokodunu istifade etmek üçün minimum {minAmount:F2} AZN lazimd?r. Cari: {actualAmount:F2} AZN");
    // USER FACTORY METHODS
    public static BusinessException InvalidCredentials()
        => new(BusinessErrorCodes.InvalidCredentials,
               "Email v? ya ?ifr? yanl??d?r.");

    public static BusinessException EmailNotConfirmed()
        => new(BusinessErrorCodes.EmailNotConfirmed,
               "Email t?sdiql?nm?yib. Z?hm?t olmasa emailinizi t?sdiql?yin.");

    public static BusinessException AccountLocked()
        => new(BusinessErrorCodes.AccountLocked,
               "Hesab?n?z kilidl?nib. Z?hm?t olmasa d?st?k il? ?laq? saxlay?n.");

    public static BusinessException VerificationCodeExpired()
        => new(BusinessErrorCodes.VerificationCodeExpired,
               "Do?rulama kodunun müdd?ti bitib. Yeni kod t?l?b edin.");

    public static BusinessException InvalidVerificationCode()
        => new(BusinessErrorCodes.InvalidVerificationCode,
               "Do?rulama kodu s?hvdir.");

    // SUBSCRIPTION FACTORY METHODS
    public static BusinessException SubscriptionExpired()
     => new(BusinessErrorCodes.SubscriptionExpired,
            "Abun?liyin müdd?ti bitib.");

    public static BusinessException SubscriptionAlreadyActive()
        => new(BusinessErrorCodes.SubscriptionAlreadyActive,
               "Art?q aktiv abun?liyiniz var.");
    // TRAINER FACTORY METHODS
    public static BusinessException TrainerAlreadyExists()
        => new(BusinessErrorCodes.TrainerAlreadyExists,
               "Bu istifad?çi art?q m??qçidir.");

    public static BusinessException TrainerNotAvailable()
        => new(BusinessErrorCodes.TrainerNotAvailable,
               "M??qçi hal-haz?rda müsait deyil.");

    // REVIEW FACTORY METHODS
    public static BusinessException AlreadyReviewed()
     => new(BusinessErrorCodes.AlreadyReviewed,
            "Bu zala art?q r?y yazm?s?n?z.");

    // CHECK-IN FACTORY METHODS
    public static BusinessException CheckInTokenExpired()
      => new(BusinessErrorCodes.CheckInTokenExpired,
             "QR kodun müdd?ti bitib. Yeni kod ?ld? edin.");

    public static BusinessException AlreadyCheckedIn()
        => new(BusinessErrorCodes.AlreadyCheckedIn,
               "Bu gün art?q giri? etmisiniz.");

    // GENERAL FACTORY METHODS
    public static BusinessException ValidationFailed(string message)
       => new(BusinessErrorCodes.ValidationFailed, message);

    public static BusinessException UnauthorizedAccess()
        => new(BusinessErrorCodes.UnauthorizedAccess,
               "Bu ?m?liyyat üçün giri? etm?lisiniz.");

    public static BusinessException Custom(BusinessErrorCodes errorCode, string message)
        => new(errorCode, message);
}
