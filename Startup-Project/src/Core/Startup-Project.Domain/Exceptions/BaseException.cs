using Startup_Project.Domain.Enums.Errors;

namespace Startup_Project.Domain.Exceptions;

public abstract class BaseException : Exception
{
    public abstract int StatusCode { get; }
    public BusinessErrorCodes ErrorCode { get; }

    protected BaseException(BusinessErrorCodes errorCode, string message) : base(message)
    {
        ErrorCode = errorCode;
    }
    protected BaseException(BusinessErrorCodes errorCode, string message, Exception innerException) 
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
