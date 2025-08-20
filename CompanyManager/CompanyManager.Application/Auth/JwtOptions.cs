namespace CompanyManager.Application.Auth
{
    public sealed class JwtOptions
    {
        public string Issuer { get; init; } = "";
        public string Audience { get; init; } = "";
        public string Secret { get; init; } = "";
        public int AccessTokenMinutes { get; init; } = 60;
        public int ClockSkewSeconds { get; init; } = 60;
    }
}
