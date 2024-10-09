namespace GoodBadHabitsTracker.Application.DTOs.Auth.Request
{
    public sealed class RegisterRequest
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
