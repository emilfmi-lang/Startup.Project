using Startup_Project.Domain.Enums;

namespace Startup_Project.Domain.Exceptions;

public class ConflictException : BaseException
{
    public override int StatusCode => 409;
    public ConflictException(BusinessErrorCodes errorCode, string message)
        : base(errorCode, message)
    {
    }
    public ConflictException(BusinessErrorCodes errorCode, string message, Exception innerException)
        : base(errorCode, message, innerException)
    {
    }
    // PROMO CODE FACTORY METHODS


    public static ConflictException PromoCodeConcurrency(string code)
        => new(BusinessErrorCodes.PromoCodeConcurrencyError,
               $"'{code}' promokodu haz?rda ba?qa istifad?çi t?r?find?n istifad? edilir. Yenid?n c?hd edin.");
    // USER FACTORY METHODS
    public static ConflictException UserAlreadyExists(string email)
       => new(BusinessErrorCodes.UserAlreadyExists,
              $"'{email}' email il? istifad?çi art?q mövcuddur.");


    // GYM FACTORY METHODS
    public static ConflictException GymAlreadyExists(string name)
        => new(BusinessErrorCodes.GymAlreadyExists,
               $"'{name}' adl? zal art?q mövcuddur.");


    // FAVORITE FACTORY METHODS
    public static ConflictException AlreadyInFavorites()
       => new(BusinessErrorCodes.AlreadyInFavorites,
              "Bu zal art?q sevimlil?rinizd?dir.");


    // SESSION FACTORY METHODS
    public static ConflictException SessionAlreadyBooked()
       => new(BusinessErrorCodes.SessionAlreadyBooked,
              "Bu sessiya art?q rezerv edilib.");
    // PAYMENT FACTORY METHODS
    public static ConflictException PaymentAlreadyProcessed()
       => new(BusinessErrorCodes.PaymentAlreadyProcessed,
              "Bu öd?ni? art?q i?l?nib.");
}
