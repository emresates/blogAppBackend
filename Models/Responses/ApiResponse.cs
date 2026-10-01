namespace BlogApi.Models.Responses;

using System.Text.Json.Serialization;

public class ApiResponse<T>
{
    public T? Data { get; set; }

    public string? Message { get; set; }

    public int StatusCode { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PaginationMeta? Pagination { get; set; }

    public static ApiResponse<T> Success(
        T data,
        int statusCode = 200,
        string? message = null,
        PaginationMeta? pagination = null
    )
    {
        return new ApiResponse<T>
        {
            Data = data,
            Message = message,
            StatusCode = statusCode,
            Pagination = pagination
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
            Message = message,
            StatusCode = statusCode,
            Pagination = null
        };
    }
}