using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GoodBadHabitsTracker.Application.Exceptions
{
    public class AppException(HttpStatusCode code, object? errors ) : Exception
    {
        public HttpStatusCode Code { get; init; } = code;
        public object? Errors { get; init; } = errors;
    }
}
