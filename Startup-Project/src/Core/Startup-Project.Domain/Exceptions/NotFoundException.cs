using Startup_Project.Domain.Enums;

namespace Startup_Project.Domain.Exceptions;

public class NotFoundException : BaseException
{
    public override int StatusCode => 404;
    public NotFoundException(BusinessErrorCodes errorCode, string message)
        : base(errorCode, message)
    {
    }
    public NotFoundException(BusinessErrorCodes errorCode, string message, Exception innerException)
        : base(errorCode, message, innerException)
    {
    }
    // USER FACTORY METHODS


    public static NotFoundException User(Guid userId)
        => new(BusinessErrorCodes.UserNotFound, $"?stifad?çi tap?lmad?. ID: {userId}");

    public static NotFoundException User(string identifier)
        => new(BusinessErrorCodes.UserNotFound, $"'{identifier}' ile istifadeçi tapilmadi.");

    public static NotFoundException UserByEmail(string email)
        => new(BusinessErrorCodes.UserNotFound, $"'{email}' email ile istifadeçi tapilmadi.");


    // GYM FACTORY METHODS


    public static NotFoundException Gym(Guid gymId)
        => new(BusinessErrorCodes.GymNotFound, $"Zal tap?lmad?. ID: {gymId}");

    public static NotFoundException Gym(string message)
        => new(BusinessErrorCodes.GymNotFound, message);

    public static NotFoundException GymForOwner()
        => new(BusinessErrorCodes.GymNotFound, "Zal?n?z tap?lmad?.");


    // PACKAGE FACTORY METHODS


    public static NotFoundException Package(Guid packageId)
        => new(BusinessErrorCodes.PackageNotFound, $"Paket tap?lmad?. ID: {packageId}");


    // PROMO CODE FACTORY METHODS


    public static NotFoundException PromoCode(string code)
        => new(BusinessErrorCodes.PromoCodeNotFound, $"'{code}' promokodu tap?lmad?.");


    // TRAINER FACTORY METHODS


    public static NotFoundException Trainer(Guid trainerId)
        => new(BusinessErrorCodes.TrainerNotFound, $"M??qçi tap?lmad?. ID: {trainerId}");


    // SUBSCRIPTION FACTORY METHODS


    public static NotFoundException Subscription(Guid subscriptionId)
        => new(BusinessErrorCodes.SubscriptionNotFound, $"Abun?lik tap?lmad?. ID: {subscriptionId}");


    // PAYMENT FACTORY METHODS


    public static NotFoundException Payment(Guid paymentId)
        => new(BusinessErrorCodes.PaymentNotFound, $"Öd?ni? tap?lmad?. ID: {paymentId}");

    public static NotFoundException Payment(string sessionId)
        => new(BusinessErrorCodes.PaymentNotFound, $"Öd?ni? tap?lmad?. Session: {sessionId}");


    // SESSION FACTORY METHODS
    public static NotFoundException Session(Guid sessionId)
              => new(BusinessErrorCodes.SessionNotFound, $"Sessiya tap?lmad?. ID: {sessionId}");


    // REVIEW FACTORY METHODS
    public static NotFoundException Review(Guid reviewId)
        => new(BusinessErrorCodes.ReviewNotFound, $"R?y tap?lmad?. ID: {reviewId}");


    // FAVORITE FACTORY METHODS
    public static NotFoundException Favorite(Guid gymId)
        => new(BusinessErrorCodes.FavoriteNotFound, $"Sevimli tap?lmad?. Gym ID: {gymId}");
}
