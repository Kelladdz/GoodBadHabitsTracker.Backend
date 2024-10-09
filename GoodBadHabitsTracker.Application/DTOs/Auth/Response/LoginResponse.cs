namespace GoodBadHabitsTracker.Application.DTOs.Auth.Response
{
     public sealed record LoginResponse(string AccessToken, string RefreshToken, string UserFingerprint);
}
