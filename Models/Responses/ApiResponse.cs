namespace BlogApi.Models.Responses;

public class ApiResponse<T>
{
    public T? Data { get; set; }

    public int StatusCode { get; set; }

    public string? Message { get; set; }


    public static ApiResponse<T> Success(
        T data,
        int statusCode = 200,
        string? message = null
    )
    {
        return new ApiResponse<T>
        {
            Data = data,
            StatusCode = statusCode,
            Message = message
        };
    }


    public static ApiResponse<T> Error(
        int statusCode,
        string message
    )
    {
        return new ApiResponse<T>
        {
            Data = default,
            StatusCode = statusCode,
            Message = message
        };
    }
}