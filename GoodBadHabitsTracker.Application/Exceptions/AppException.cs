using System.Net;

namespace GoodBadHabitsTracker.Application.Exceptions
{
    public sealed class AppException(HttpStatusCode code, object? errors ) : Exception
    {
        public HttpStatusCode Code { get; init; } = code;
        public object? Errors { get; init; } = errors;
    }
}
