using System.Net;
using System.Text.Json;
using GoodBadHabitsTracker.Application.Exceptions;

namespace GoodBadHabitsTracker.WebApi.Middleware
{
    public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        private static readonly Action<ILogger, string, Exception> LOGGER_MESSAGE =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            eventId: new EventId(id: 0, name: "ERROR"),
            formatString: "{Message}"
        );

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await next(httpContext);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(httpContext, ex, logger);
            }
        }
        private static async Task HandleExceptionAsync(
        HttpContext context,
        Exception exception,
        ILogger<ExceptionHandlingMiddleware> logger
    )
        {
            string? result;
            switch (exception)
            {
                case AppException re:
                    context.Response.StatusCode = (int)re.Code;
                    result = JsonSerializer.Serialize(new { errors = re.Errors });
                    break;
                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    LOGGER_MESSAGE(logger, "Unhandled Exception", exception);
                    result = JsonSerializer.Serialize(
                        new { errors = "InternalServerError" }
                    );
                    break;
            }

            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(result);
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
