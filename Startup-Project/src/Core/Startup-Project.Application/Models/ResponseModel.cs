namespace Startup_Project.Application.Models;

public class ResponseModel<T>
{
    public T? Data { get; set; }
    public bool Success { get; set; }
    public List<string>? Errors { get; set; }
    public static ResponseModel<T> SuccessResponse(T data)
    {
        return new ResponseModel<T>
        {
            Data = data,
            Success = true,
            Errors = null
        };
    }
    public static ResponseModel<T> FailureResponse(string error)
    {
        return new ResponseModel<T>
        {
            Success = false,
            Errors = [error]
        };
    }

    public static ResponseModel<T> FailureResponse(List<string> errors)
    {
        return new ResponseModel<T>
        {
            Success = false,
            Errors = errors
        };
    }
}
