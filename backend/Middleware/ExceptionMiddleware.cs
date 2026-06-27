using System.Text.Json;
using PlayLeague.Api.Exceptions;

namespace PlayLeague.Api.Middleware;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await next(ctx);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await WriteError(ctx, ex);
        }
    }

    private static Task WriteError(HttpContext ctx, Exception ex)
    {
        var (status, message) = ex switch
        {
            ConflictException e     => (StatusCodes.Status409Conflict, e.Message),
            UnauthorizedException e => (StatusCodes.Status401Unauthorized, e.Message),
            ForbiddenException e    => (StatusCodes.Status403Forbidden, e.Message),
            NotFoundException e     => (StatusCodes.Status404NotFound, e.Message),
            _                       => (StatusCodes.Status500InternalServerError, "An unexpected error occurred."),
        };

        ctx.Response.StatusCode = status;
        ctx.Response.ContentType = "application/json";
        return ctx.Response.WriteAsync(JsonSerializer.Serialize(new { error = message }));
    }
}
