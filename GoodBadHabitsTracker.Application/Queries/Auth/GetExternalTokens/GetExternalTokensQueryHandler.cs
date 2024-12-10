using MediatR;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using GoodBadHabitsTracker.Application.DTOs.Response;
using LanguageExt.Common;
using GoodBadHabitsTracker.Application.Exceptions;
using System.Net;

namespace GoodBadHabitsTracker.Application.Queries.Auth.GetExternalTokens
{
    internal sealed class GetExternalTokensQueryHandler(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration) : IRequestHandler<GetExternalTokensQuery, Result<GetExternalTokensResponse>>
    {
        public async Task<Result<GetExternalTokensResponse>> Handle(GetExternalTokensQuery query, CancellationToken cancellationToken)
        {
            if (query.Request is null) 
                return new Result<GetExternalTokensResponse>(new AppException(HttpStatusCode.BadRequest, "Request cannot be null."));

            if (query.Request.GrantType is null ||
                query.Request.Code is null ||
                query.Request.RedirectUri is null ||
                query.Request.ClientId is null ||
                query.Request.CodeVerifier is null)
                 return new Result<GetExternalTokensResponse>(new AppException(HttpStatusCode.BadRequest, "Any request data cannot be null."));

            if (query.Provider is null || (query.Provider != "Google" && query.Provider != "Facebook"))
                return new Result<GetExternalTokensResponse>(new AppException(HttpStatusCode.BadRequest, "Provider is not correct."));

            var client = httpClientFactory.CreateClient();
            var values = new Dictionary<string, string>
            {
                 { "grant_type", query.Request.GrantType },
                 { "code", query.Request.Code },
                 { "redirect_uri", query.Request.RedirectUri },
                 { "client_id", query.Request.ClientId },
                 { "code_verifier", query.Request.CodeVerifier },
                 { "client_secret", configuration["JwtSettings:ClientSecret"]! }
            };

            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.PostAsync("https://dev-d3sgzf7qkeuvnndt.eu.auth0.com/oauth/token", new FormUrlEncodedContent(values), cancellationToken);
            if (!response.IsSuccessStatusCode)
                return new Result<GetExternalTokensResponse>(new AppException(response.StatusCode, "Response is not successful."));

            var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            

            var jsonDocument = JsonDocument.Parse(responseString);
            if (jsonDocument is null)
                return new Result<GetExternalTokensResponse>(new AppException(HttpStatusCode.BadRequest, "Json document cannot be null."));

            var getExternalTokensResponse = new GetExternalTokensResponse
            {
                AccessToken = jsonDocument.RootElement.GetProperty("access_token").GetString()!,
                ExpiresIn = jsonDocument.RootElement.GetProperty("expires_in").GetInt32()!,
                Scope = jsonDocument.RootElement.GetProperty("scope").GetString()!,
                IdToken = jsonDocument.RootElement.GetProperty("id_token").GetString()!,
                TokenType = jsonDocument.RootElement.GetProperty("token_type").GetString()!,
                Provider = query.Provider
            };

            if (jsonDocument.RootElement.TryGetProperty("refresh_token", out JsonElement json))
                getExternalTokensResponse.RefreshToken = json.GetString();

            return new Result<GetExternalTokensResponse>(getExternalTokensResponse);
        }
    }
}
