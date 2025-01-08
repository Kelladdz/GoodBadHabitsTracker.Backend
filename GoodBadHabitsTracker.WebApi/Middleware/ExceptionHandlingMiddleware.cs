using GoodBadHabitsTracker.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

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
                logger.LogError(ex, "Exception occurred: {Message}", ex.Message);

                var exceptionDetails = GetExceptionDetails(ex);

                var problemDetails = new ProblemDetails
                {
                    Status = exceptionDetails.Status,
                    Type = exceptionDetails.Type,
                    Title = exceptionDetails.Title,
                    Detail = exceptionDetails.Detail
                };

                if (exceptionDetails.Errors is not null)
                {
                    problemDetails.Extensions["errors"] = exceptionDetails.Errors;
                }

                httpContext.Response.StatusCode = exceptionDetails.Status;
                await httpContext.Response.WriteAsJsonAsync(problemDetails);
            }
        }
        private static ExceptionDetails GetExceptionDetails(Exception exception)
        {
            return exception switch
            {
                AppException appException => new ExceptionDetails(
                    (int)appException.Code,
                    "ApplicationFailure",
                    "Application error",
                    appException.Errors!.ToString()!,
                    appException.Errors),
                ValidationException validationException => new ExceptionDetails(
                    StatusCodes.Status400BadRequest,
                    "ValidationFailure",
                    "Validation error",
                    "One or more validation errors has occurred",
                    validationException.Errors),
                _ => new ExceptionDetails(
                    StatusCodes.Status500InternalServerError,
                    "ServerError",
                    "Server error",
                    "An unexpected error has occurred",
                    null)
            };
        }
    }

    public record ExceptionDetails(int Status, string Type, string Title, string Detail, object? Errors);

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}