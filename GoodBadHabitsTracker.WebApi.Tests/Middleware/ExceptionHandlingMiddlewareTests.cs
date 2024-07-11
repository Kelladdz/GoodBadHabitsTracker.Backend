using FluentAssertions;
using GoodBadHabitsTracker.WebApi.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GoodBadHabitsTracker.Application.Exceptions;
using System.Net;
using Xunit.Sdk;
using System.Text.Json;

namespace GoodBadHabitsTracker.WebApi.Tests.Middleware
{
    public class ExceptionHandlingMiddlewareTests
    {
        private readonly DefaultHttpContext _httpContext;
        private readonly Mock<RequestDelegate> _nextMock;
        private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _loggerMock;
        private readonly ExceptionHandlingMiddleware _middleware;
        private readonly Mock<ExceptionHandlingMiddleware> _middlewareMock;

        private static readonly Action<ILogger, string, Exception> LOGGER_MESSAGE =
        LoggerMessage.Define<string>(
            LogLevel.Error,
            eventId: new EventId(id: 0, name: "ERROR"),
            formatString: "{Message}"
        );
        private readonly Array _httpStatusCodes = Enum.GetValues(typeof(HttpStatusCode));

        public ExceptionHandlingMiddlewareTests()
        {
            _httpContext = new DefaultHttpContext();
            _nextMock = new Mock<RequestDelegate>();
            _loggerMock = new Mock<ILogger<ExceptionHandlingMiddleware>>();
            _middleware = new ExceptionHandlingMiddleware(_nextMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task Invoke_NoExceptions_PassRequestToNextMiddleware()
        {
            //ARRANGE
            _nextMock.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

            //ACT
            await _middleware.InvokeAsync(_httpContext);

            //ASSERT
            _nextMock.Verify(x => x(It.IsAny<HttpContext>()), Times.Once);
        }

        [Fact]
        public async Task Invoke_ThrowsAppException_ReturnsStatusCode_AndDoesntPassRequestToNextMiddleware()
        {
            //ARRANGE
            var random = new Random();
            var statusCode = (HttpStatusCode)_httpStatusCodes.GetValue(random.Next(_httpStatusCodes.Length));
            var error = "Exception";
            var exception = new AppException(statusCode, error);
            _httpContext.Response.Body = new MemoryStream();

            _nextMock.Setup(x => x(It.IsAny<HttpContext>())).Throws(exception);

            //ACT
            await _middleware.InvokeAsync(_httpContext);

            //ASSERT
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseContent = new StreamReader(_httpContext.Response.Body).ReadToEnd();
            var expectedResponse = JsonSerializer.Serialize(new 
            { 
                type = "ApplicationFailure",
                title = "Application error",
                status = statusCode,
                detail = "Something goes wrong",
                errors = error
            });

            _httpContext.Response.StatusCode.Should().Be((int)statusCode);
            responseContent.Should().BeEquivalentTo(expectedResponse);

            _nextMock.Verify(x => x(It.IsAny<HttpContext>()), Times.Once);
        }
        [Fact]
        public async Task Invoke_ThrowsUnhandledException_ReturnsInternalServerError_AndDoesntPassRequestToNextMiddleware()
        {
            //ARRANGE
            var random = new Random();
            var statusCode = HttpStatusCode.InternalServerError;
            var exception = new Exception(null);
            _httpContext.Response.Body = new MemoryStream();

            _nextMock.Setup(x => x(It.IsAny<HttpContext>())).Throws(exception);

            //ACT
            await _middleware.InvokeAsync(_httpContext);

            //ASSERT
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var responseContent = new StreamReader(_httpContext.Response.Body).ReadToEnd();
            var expectedResponse = JsonSerializer.Serialize(new
            {
                type = "ServerError",
                title = "Server error",
                status = statusCode,
                detail = "An unexpected error has occurred"
            });

            _httpContext.Response.StatusCode.Should().Be((int)statusCode);
            responseContent.Should().BeEquivalentTo(expectedResponse);

            _nextMock.Verify(x => x(It.IsAny<HttpContext>()), Times.Once);
        }
    }
}
