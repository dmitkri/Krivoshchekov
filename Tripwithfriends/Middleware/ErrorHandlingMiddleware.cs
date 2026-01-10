using System.Net;
using System.Text.Json;

namespace Tripwithfriends.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Произошла ошибка: {Message}", ex.Message);
            
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        HttpStatusCode code;
        
        if (exception is KeyNotFoundException)
        {
            code = HttpStatusCode.NotFound;
        }
        else if (exception is UnauthorizedAccessException)
        {
            code = HttpStatusCode.Unauthorized;
        }
        else if (exception is InvalidOperationException || exception is ArgumentException)
        {
            code = HttpStatusCode.BadRequest;
        }
        else
        {
            code = HttpStatusCode.InternalServerError;
        }

        var errorResponse = new
        {
            error = exception.Message,
            statusCode = (int)code
        };

        var result = JsonSerializer.Serialize(errorResponse);

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)code;

        return context.Response.WriteAsync(result);
    }
}
