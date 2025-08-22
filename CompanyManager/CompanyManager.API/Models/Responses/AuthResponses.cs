using CompanyManager.API.Models;

namespace CompanyManager.API.Models.Responses
{
    public class AuthenticationResponse : BaseResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string TokenType { get; set; } = string.Empty;
        public UserInfo? User { get; set; }
    }

    public class UserInfo
    {
        public Guid Id { get; set; }
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
        public UserJobTitleInfo? JobTitle { get; set; }
        public RoleInfo? Role { get; set; }
    }

    public class UserJobTitleInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int HierarchyLevel { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class RoleInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
    }


}
