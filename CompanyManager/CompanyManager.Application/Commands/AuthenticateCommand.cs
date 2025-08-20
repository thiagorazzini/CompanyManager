using System;

namespace CompanyManager.Application.Commands
{
    public sealed class AuthenticateCommand
    {
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}
