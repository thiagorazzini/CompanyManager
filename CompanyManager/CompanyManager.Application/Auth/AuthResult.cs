using System;

namespace CompanyManager.Application.Auth
{
    /// <summary>
    /// Represents the result of an authentication operation
    /// </summary>
    /// <param name="AccessToken">The JWT access token</param>
    /// <param name="ExpiresAt">When the token expires</param>
    public sealed record AuthResult(string AccessToken, DateTime ExpiresAt);
}
