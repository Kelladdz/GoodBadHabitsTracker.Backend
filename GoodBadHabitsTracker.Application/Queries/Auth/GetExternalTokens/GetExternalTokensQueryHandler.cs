using MediatR;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using GoodBadHabitsTracker.Application.DTOs.Response;

namespace GoodBadHabitsTracker.Application.Queries.Auth.GetExternalTokens
{
    internal sealed class GetExternalTokensQueryHandler(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration) : IRequestHandler<GetExternalTokensQuery, GetExternalTokensResponse>
    {
        public async Task<GetExternalTokensResponse> Handle(GetExternalTokensQuery query, CancellationToken cancellationToken)
        {
            if (query.Request is null) throw new HttpRequestException("Request cannot be null.");

            if (query.Request.GrantType is null ||
                query.Request.Code is null ||
                query.Request.RedirectUri is null ||
                query.Request.ClientId is null ||
                query.Request.CodeVerifier is null) throw new HttpRequestException("Any request data cannot be null.");

            if (query.Provider is null || (query.Provider != "Google" && query.Provider != "Facebook")) throw new HttpRequestException("Provider is not correct.");

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
            if (!response.IsSuccessStatusCode) throw new HttpRequestException("Response is not successful.");

            var responseString = await response.Content.ReadAsStringAsync()
                ?? throw new InvalidOperationException("Response string cannot be null.");

            var jsonDocument = JsonDocument.Parse(responseString)
                ?? throw new JsonException("Json document cannot be null.");

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

            return getExternalTokensResponse;
        }
    }
}
