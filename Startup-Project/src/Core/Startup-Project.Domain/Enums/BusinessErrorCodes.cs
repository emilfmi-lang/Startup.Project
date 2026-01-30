
namespace Startup_Project.Domain.Enums;

public enum BusinessErrorCodes
{
    UnknownError = 1000,
    ValidationFailed = 1001,
    UnauthorizedAccess = 1002,
    ForbiddenAccess = 1003,

    UserNotFound = 2000,
    UserAlreadyExists = 2001,
    InvalidCredentials = 2002,
    EmailNotConfirmed = 2003,
    PhoneNotConfirmed = 2004,
    AccountLocked = 2005,
    InvalidVerificationCode = 2006,
    VerificationCodeExpired = 2007,
}
