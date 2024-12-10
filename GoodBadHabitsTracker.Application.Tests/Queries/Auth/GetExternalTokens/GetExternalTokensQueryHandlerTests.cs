using FluentAssertions;
using GoodBadHabitsTracker.Application.DTOs.Request;
using GoodBadHabitsTracker.Application.Exceptions;
using GoodBadHabitsTracker.Application.Queries.Auth.GetExternalTokens;
using Moq.Protected;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using GoodBadHabitsTracker.TestMisc;
using System.Text.Json;

namespace GoodBadHabitsTracker.Application.Tests.Queries.Auth.GetExternalTokens
{
    public class GetExternalTokensQueryHandlerTests
    {
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly GetExternalTokensQueryHandler _handler;

        public GetExternalTokensQueryHandlerTests()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _configurationMock = new Mock<IConfiguration>();
            _handler = new GetExternalTokensQueryHandler(_httpClientFactoryMock.Object, _configurationMock.Object);
        }

        [Fact]
        public async Task Handle_ValidRequest_ReturnsSuccess()
        {
            //ARRANGE
            var request = DataGenerator.SeedGetExternalTokensRequest();
            var query = new GetExternalTokensQuery(request, "Google");

            var responseContent = new
            {
                access_token = "accessToken",
                expires_in = 3600,
                scope = "scope",
                id_token = "idToken",
                token_type = "tokenType",
                refresh_token = "refreshToken"
            };

            var httpClientMock = new Mock<HttpMessageHandler>();
            httpClientMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(JsonSerializer.Serialize(responseContent))
                });

            var client = new HttpClient(httpClientMock.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.IfSucc(response =>
            {
                response.AccessToken.Should().Be(responseContent.access_token);
                response.ExpiresIn.Should().Be(responseContent.expires_in);
                response.Scope.Should().Be(responseContent.scope);
                response.IdToken.Should().Be(responseContent.id_token);
                response.TokenType.Should().Be(responseContent.token_type);
                response.RefreshToken.Should().Be(responseContent.refresh_token);
                response.Provider.Should().Be(query.Provider);
            });
        }
        [Fact]
        public async Task Handle_RequestIsNull_ReturnsBadRequest()
        {
            //ARRANGE
            var query = new GetExternalTokensQuery(null, "Google");

            //ACT
            var result = await _handler.Handle(query, CancellationToken.None);

            //ASSERT
            result.IsFaulted.Should().BeTrue(); ;
        }

        [Fact]
        public async Task Handle_AnyRequestDataIsNull_ReturnsBadRequest()
        {
            //ARRANGE
            var request = new GetExternalTokensRequest
            {
                GrantType = null,
                Code = "code",
                RedirectUri = "redirectUri",
                ClientId = "clientId",
                CodeVerifier = "codeVerifier"
            };
            var query = new GetExternalTokensQuery(request, "Google");

            //ACT
            var result = await _handler.Handle(query, CancellationToken.None);

            //ASSERT
            result.IsFaulted.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ProviderIsNotCorrect_ReturnsBadRequest()
        {
            //ARRANGE
            var request = DataGenerator.SeedGetExternalTokensRequest();
            var query = new GetExternalTokensQuery(request, null);

            //ACT
            var result = await _handler.Handle(query, CancellationToken.None);

            //ASSERT
            result.IsFaulted.Should().BeTrue();
            //result.IfFail(ex => ex.Should().BeOfType<AppException>()
            //.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest));
        }

        [Fact]
        public async Task Handle_ResponseIsNotSuccessful_ReturnsBadRequest()
        {
            //ARRANGE
            var request = DataGenerator.SeedGetExternalTokensRequest();
            var query = new GetExternalTokensQuery(request, "Google");

            var httpClientMock = new Mock<HttpMessageHandler>();
            httpClientMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var client = new HttpClient(httpClientMock.Object);
            _httpClientFactoryMock.Setup(_ => _.CreateClient(It.IsAny<string>())).Returns(client);

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.IsFaulted.Should().BeTrue();
        }

       
    }
}
