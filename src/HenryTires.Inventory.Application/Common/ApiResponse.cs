namespace HenryTires.Inventory.Application.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public string? DeveloperMessage { get; set; }

    public static ApiResponse<T> SuccessResponse(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            ErrorMessage = null,
            DeveloperMessage = null,
        };
    }

    public static ApiResponse<T> ErrorResponse(string errorMessage, string? developerMessage = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Data = default,
            ErrorMessage = errorMessage,
            DeveloperMessage = developerMessage,
        };
    }
}
