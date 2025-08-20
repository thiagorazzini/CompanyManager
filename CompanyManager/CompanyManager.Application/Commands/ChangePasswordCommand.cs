namespace CompanyManager.Application.Commands
{
    public sealed class ChangePasswordCommand
    {
        public string Email { get; init; } = string.Empty;
        public string CurrentPassword { get; init; } = string.Empty;
        public string NewPassword { get; init; } = string.Empty;
        public string ConfirmNewPassword { get; init; } = string.Empty;
    }
}
