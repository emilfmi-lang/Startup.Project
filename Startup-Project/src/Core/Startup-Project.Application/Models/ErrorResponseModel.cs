using Startup_Project.Domain.Enums.Errors;
using System.Collections.Concurrent;

namespace Startup_Project.Application.Models;

public class ErrorResponseModel
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public int? ErrorNumber { get; set; }
    public List<string> Errors { get; set; } = [];

    public static ErrorResponseModel FromException(BusinessErrorCodes errorCode, string message)
    {
        return new ErrorResponseModel
        {
            Success = false,
            ErrorCode = errorCode.ToSnakeCase(),
            ErrorNumber = (int)errorCode,
            Errors = [message]
        };
    }

    public static ErrorResponseModel FromException(BusinessErrorCodes errorCode, List<string> messages)
    {
        return new ErrorResponseModel
        {
            Success = false,
            ErrorCode = errorCode.ToSnakeCase(),
            ErrorNumber = (int)errorCode,
            Errors = messages
        };
    }

    public static ErrorResponseModel InternalError()
    {
        return new ErrorResponseModel
        {
            Success = false,
            ErrorCode = "INTERNAL_SERVER_ERROR",
            ErrorNumber = 500,
            Errors = ["Gözlənilməz bir xəta baş verdi."]
        };
    }
}

public static class EnumExtensions
{
    private static readonly ConcurrentDictionary<BusinessErrorCodes, string> _snakeCaseCache = new();

    public static string ToSnakeCase(this BusinessErrorCodes errorCode)
    {
        return _snakeCaseCache.GetOrAdd(errorCode, code =>
        {
            var value = code.ToString();
            if (string.IsNullOrEmpty(value)) return value;

            var result = new System.Text.StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                if (char.IsUpper(value[i]) && i > 0)
                    result.Append('_');
                result.Append(char.ToUpper(value[i]));
            }
            return result.ToString();
        });
    }
}