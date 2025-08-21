using System;

namespace CompanyManager.Application.Auth
{
    /// <summary>
    /// Represents the result of an authentication operation
    /// </summary>
    /// <param name="AccessToken">The JWT access token</param>
    /// <param name="ExpiresAt">When the token expires</param>
    /// <param name="UserId">The ID of the authenticated user</param>
    /// <param name="Email">The email of the authenticated user</param>
    public sealed record AuthResult(string AccessToken, DateTime ExpiresAt, Guid UserId, string Email);
}
