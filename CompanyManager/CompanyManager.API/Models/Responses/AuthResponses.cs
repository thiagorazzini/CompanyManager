using CompanyManager.API.Models;

namespace CompanyManager.API.Models.Responses
{
    public class AuthenticationResponse : BaseResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string TokenType { get; set; } = string.Empty;
        public UserInfo? User { get; set; }
    }

    public class UserInfo
    {
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }

    public class PasswordChangedResponse : BaseResponse
    {
    }

    public class LogoutResponse : BaseResponse
    {
    }

    public class UserProfileResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastLoginAt { get; set; }
    }

    public class LogoutRequest
    {
        public string? RefreshToken { get; set; }
    }
}
