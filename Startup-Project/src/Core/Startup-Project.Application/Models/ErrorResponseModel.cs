
using Startup_Project.Domain.Enums;

namespace Startup_Project.Application.Models;

public class ErrorResponseModel
{
    public bool Success { get; set; }
    public string? ErrorCode { get; set; }
    public int? ErrorNumber { get; set; }
    public List<string> Errors { get; set; } = [];
    public static ErrorResponseModel FromException (BusinessErrorCodes businessErrorCodes, string message)
    {
        return new ErrorResponseModel
        {
            Success = false,
            ErrorCode = businessErrorCodes.ToString(),
            ErrorNumber = (int)businessErrorCodes,
            Errors = [message]
        };
    }
}
