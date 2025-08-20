using CompanyManager.Application.Abstractions;
using CompanyManager.Application.Commands;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Auth;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CompanyManager.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticateCommandHandler _authenticateHandler;
        private readonly IRefreshTokenCommandHandler _refreshTokenHandler;
        private readonly IChangePasswordCommandHandler _changePasswordHandler;

        public AuthController(
            IAuthenticateCommandHandler authenticateHandler,
            IRefreshTokenCommandHandler refreshTokenHandler,
            IChangePasswordCommandHandler changePasswordHandler)
        {
            _authenticateHandler = authenticateHandler ?? throw new ArgumentNullException(nameof(authenticateHandler));
            _refreshTokenHandler = refreshTokenHandler ?? throw new ArgumentNullException(nameof(refreshTokenHandler));
            _changePasswordHandler = changePasswordHandler ?? throw new ArgumentNullException(nameof(changePasswordHandler));
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token
        /// </summary>
        /// <param name="request">Authentication credentials</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Authentication result with JWT token</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login(
            [FromBody] AuthenticateRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var command = new AuthenticateCommand
                {
                    Email = request.Email,
                    Password = request.Password
                };

                var result = await _authenticateHandler.Handle(command, cancellationToken);

                return Ok(new
                {
                    message = "Authentication successful",
                    accessToken = result.AccessToken,
                    expiresAt = result.ExpiresAt
                });
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors.Select(e => e.ErrorMessage);
                return BadRequest(new { errors });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "An error occurred during authentication" });
            }
        }

        /// <summary>
        /// Refreshes an access token using a refresh token
        /// </summary>
        /// <param name="request">Refresh token data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>New authentication result with JWT token</returns>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken(
            [FromBody] RefreshTokenRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var command = new RefreshTokenCommand
                {
                    RefreshToken = request.RefreshToken
                };

                var result = await _refreshTokenHandler.Handle(command, cancellationToken);

                return Ok(new
                {
                    message = "Token refreshed successfully",
                    accessToken = result.AccessToken,
                    expiresAt = result.ExpiresAt
                });
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors.Select(e => e.ErrorMessage);
                return BadRequest(new { errors });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "Invalid refresh token" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "An error occurred while refreshing the token" });
            }
        }

        /// <summary>
        /// Changes a user's password
        /// </summary>
        /// <param name="request">Password change data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success message</returns>
        [HttpPost("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordRequest request,
            CancellationToken cancellationToken)
        {
            try
            {
                var command = new ChangePasswordCommand
                {
                    Email = request.Email,
                    CurrentPassword = request.CurrentPassword,
                    NewPassword = request.NewPassword,
                    ConfirmNewPassword = request.ConfirmNewPassword
                };

                await _changePasswordHandler.Handle(command, cancellationToken);

                return Ok(new { message = "Password changed successfully" });
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors.Select(e => e.ErrorMessage);
                return BadRequest(new { errors });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { error = "Invalid credentials" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = "An error occurred while changing the password" });
            }
        }
    }
}
