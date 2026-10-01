using System.Net;
using System.Text.Json;
using BlogApi.Exceptions;
using BlogApi.Models.Responses;

namespace BlogApi.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            await HandleAppException(context, ex);
        }
        catch (Exception)
        {
            await HandleUnknownException(context);
        }
    }

    private static async Task HandleAppException(
        HttpContext context,
        AppException ex
    )
    {
        context.Response.StatusCode = ex.StatusCode;
        context.Response.ContentType = "application/json";

        var response = ApiResponse<object>.Error(
            ex.StatusCode,
            ex.Message,
            ex.ErrCode
        );

        var json = JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }

    private static async Task HandleUnknownException(
        HttpContext context
    )
    {
        context.Response.StatusCode =
            (int)HttpStatusCode.InternalServerError;

        context.Response.ContentType =
            "application/json";

        var response = ApiResponse<object>.Error(
            500,
            "Beklenmeyen bir hata oluştu.",
            "internalServerError"
        );

        var json = JsonSerializer.Serialize(response);

        await context.Response.WriteAsync(json);
    }
}