namespace GoodBadHabitsTracker.Application.DTOs.Response
{
    public sealed record LoginResponse(string AccessToken, string RefreshToken, string UserFingerprint);
}
