namespace GoodBadHabitsTracker.Infrastructure.Settings
{
    public struct DevelopmentPaths
    {
        public const string CONFIRM_EMAIL_CALLBACK = "https://localhost:8080/auth/confirm-email/callback";
        public const string RESET_PASSWORD_CALLBACK = "https://localhost:8080/auth/reset-password/callback";
    }

    public struct CookieNames
    {
        public const string USER_FINGERPRINT_COOKIE_NAME = "__Secure-Fgp";
        public const string REFRESH_TOKEN_COOKIE_NAME = "__Secure-Rt";
    }

    public struct EmailSubjects
    {
        public const string WELCOME = "Welcome To GoodBadHabitsTracker!";
        public const string PASSWORD_RESET = "Password Reset Request";
        public const string CONGRATULATIONS = "Congratulations";
    }

    public struct CustomClaimNames
    {
        public const string USER_FINGERPRINT = "userFingerprint";
        public const string ROLES = "roles";
    }

    public struct ExternalTokenProviders
    {
        public const string GOOGLE = "Google";
        public const string FACEBOOK = "Facebook";
    }

    public struct QuartzJobKeys
    {
        public const string FILL_PAST_DAYS_JOB_KEY = "FillPastDaysJob";
    }

    public struct Policies
    {
        public const string AUTHORIZATION_POLICY_NAME = "GBHTPolicy";
    }

    public struct AuthenticationSchemas
    {
        public const string EMAIL_PASSWORD_AUTHENTICATION_SCHEMA = "PasswordLogin";
        public const string AUTH0_AUTHENTICATION_SCHEMA = "Auth0Login";
    }
}
