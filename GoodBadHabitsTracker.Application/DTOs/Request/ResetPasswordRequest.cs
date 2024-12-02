namespace GoodBadHabitsTracker.Application.DTOs.Request
{
    public class ResetPasswordRequest
    {
        public string Password { get; init; } = default!;
        public string Token { get; init; } = default!;
        public string Email { get; init; } = default!;
    }
}
