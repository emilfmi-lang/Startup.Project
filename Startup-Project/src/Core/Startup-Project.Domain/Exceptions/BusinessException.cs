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
               "Email və ya şifrə yanlışdır.");

    // AUTH PROVIDER FACTORY METHODS
    public static BusinessException GoogleTokenInvalid()
        => new(BusinessErrorCodes.GoogleTokenInvalid,
               "Google token etibarsızdır.");

    public static BusinessException GoogleEmailNotVerified()
        => new(BusinessErrorCodes.GoogleEmailNotVerified,
               "Google hesabınızın emaili təsdiq olunmayıb.");

    public static BusinessException GoogleAuthRequired()
        => new(BusinessErrorCodes.GoogleAuthRequired,
               "Bu hesab Google ilə qeydiyyatdan keçib. Zəhmət olmasa Google ilə daxil olun.");

    public static BusinessException PhoneAuthRequired()
        => new(BusinessErrorCodes.PhoneAuthRequired,
               "Bu hesab telefon nömrəsi ilə qeydiyyatdan keçib. Zəhmət olmasa SMS OTP ilə daxil olun.");

    public static BusinessException PasswordResetNotAllowed(string provider)
        => new(BusinessErrorCodes.PasswordResetNotAllowed,
               $"{provider} ilə qeydiyyatdan keçmiş hesab üçün şifrə sıfırlama mümkün deyil.");

    // OTP FACTORY METHODS
    public static BusinessException OtpNotFound()
        => new(BusinessErrorCodes.OtpNotFound,
               "Aktiv doğrulama kodu tapılmadı. Yeni kod tələb edin.");

    public static BusinessException OtpInvalidCode(int remainingAttempts)
        => new(BusinessErrorCodes.OtpInvalidCode,
               $"Kod səhvdir. {remainingAttempts} cəhd qalıb.");

    public static BusinessException OtpMaxAttemptsExceeded()
        => new(BusinessErrorCodes.OtpMaxAttemptsExceeded,
               "3 səhv cəhd. Kod ləğv edildi. Yeni kod tələb edin.");

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

        // ════════════════════════════════════════════════════════════════
        // CAMPAIGN FACTORY METHODS
        // ════════════════════════════════════════════════════════════════

        public static BusinessException CampaignExpired(string name, DateTime endDate)
            => new(BusinessErrorCodes.CampaignExpired,
                   $"'{name}' kampaniyasının müddəti bitib. Son tarix: {endDate:dd.MM.yyyy}");

        public static BusinessException CampaignDeactivated(string name)
            => new(BusinessErrorCodes.CampaignDeactivated,
                   $"'{name}' kampaniyası deaktiv edilib.");

        public static BusinessException CampaignUsageLimitExceeded(string name, int limit)
            => new(BusinessErrorCodes.CampaignUsageLimitExceeded,
                   $"'{name}' kampaniyasının istifadə limiti ({limit}) dolub.");

        public static BusinessException CampaignNotApplicable(string name)
            => new(BusinessErrorCodes.CampaignNotApplicable,
                   $"'{name}' kampaniyası bu şərtlərə uyğun deyil.");

        public static BusinessException CampaignMinimumAmountNotMet(string name, decimal minAmount, decimal actualAmount)
            => new(BusinessErrorCodes.CampaignMinimumAmountNotMet,
                   $"'{name}' kampaniyası üçün minimum {minAmount:F2} AZN lazımdır. Cari: {actualAmount:F2} AZN");

        public static BusinessException CampaignRuleEvaluationFailed(string ruleName)
            => new(BusinessErrorCodes.CampaignRuleEvaluationFailed,
                   $"Kampaniya qaydası '{ruleName}' yerinə yetirilmədi.");

        public static BusinessException CampaignAlreadyExists(string name)
            => new(BusinessErrorCodes.CampaignAlreadyExists,
                   $"'{name}' adlı kampaniya artıq mövcuddur.");

        // ════════════════════════════════════════════════════════════════
        // PRICING FACTORY METHODS
        // ════════════════════════════════════════════════════════════════

        public static BusinessException PricingCalculationFailed(string reason)
            => new(BusinessErrorCodes.PricingCalculationFailed,
                   $"Qiymət hesablanması uğursuz oldu: {reason}");

        public static BusinessException InvalidPricingContext()
            => new(BusinessErrorCodes.InvalidPricingContext,
                   "Qiymət hesablaması üçün kontekst düzgün deyil.");
    }
