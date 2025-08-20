using System;

namespace CompanyManager.Application.DTOs
{
    public sealed class AuthenticateRequest
    {
        public string Email { get; init; } = string.Empty;
        public string Password { get; init; } = string.Empty;
    }
}
