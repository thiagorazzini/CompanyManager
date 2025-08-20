namespace CompanyManager.Application.Commands
{
    public sealed class RefreshTokenCommand
    {
        public string RefreshToken { get; init; } = string.Empty;
    }
}
