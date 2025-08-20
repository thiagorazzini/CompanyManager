using CompanyManager.Application.Abstractions;
using CompanyManager.Application.Commands;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Auth;
using CompanyManager.API.Models;
using CompanyManager.API.Models.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace CompanyManager.API.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticateCommandHandler _authenticateHandler;
        private readonly IRefreshTokenCommandHandler _refreshTokenHandler;
        private readonly IChangePasswordCommandHandler _changePasswordHandler;
        private readonly ILogoutCommandHandler _logoutHandler;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthenticateCommandHandler authenticateHandler,
            IRefreshTokenCommandHandler refreshTokenHandler,
            IChangePasswordCommandHandler changePasswordHandler,
            ILogoutCommandHandler logoutHandler,
            ICurrentUserService currentUserService,
            ILogger<AuthController> logger)
        {
            _authenticateHandler = authenticateHandler ?? throw new ArgumentNullException(nameof(authenticateHandler));
            _refreshTokenHandler = refreshTokenHandler ?? throw new ArgumentNullException(nameof(refreshTokenHandler));
            _changePasswordHandler = changePasswordHandler ?? throw new ArgumentNullException(nameof(changePasswordHandler));
            _logoutHandler = logoutHandler ?? throw new ArgumentNullException(nameof(logoutHandler));
            _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token
        /// </summary>
        /// <param name="request">Authentication credentials</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Authentication result with JWT token</returns>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> Login(
            [FromBody] AuthenticateRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest(new ErrorResponse("Request body is required"));

            try
            {
                var command = new AuthenticateCommand
                {
                    Email = request.Email,
                    Password = request.Password
                };

                var result = await _authenticateHandler.Handle(command, cancellationToken);

                var response = new AuthenticationResponse
                {
                    Message = "Authentication successful",
                    AccessToken = result.AccessToken,
                    RefreshToken = null,
                    ExpiresAt = result.ExpiresAt,
                    TokenType = "Bearer",
                    User = new UserInfo
                    {
                        Email = request.Email
                    }
                };

                return Ok(response);
            }
            catch (ValidationException ex)
            {
                var response = new ValidationErrorResponse("Validation failed")
                {
                    Errors = ex.Errors.Select(e => e.ErrorMessage).ToList()
                };
                return BadRequest(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new ErrorResponse("Invalid credentials"));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("account locked"))
            {
                return StatusCode(423, new ErrorResponse("Account is locked"));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("too many attempts"))
            {
                return StatusCode(429, new ErrorResponse("Too many login attempts. Please try again later."));
            }
            catch (Exception)
            {
                return StatusCode(500, new ErrorResponse("An error occurred during authentication"));
            }
        }

        /// <summary>
        /// Refreshes an access token using a refresh token
        /// </summary>
        /// <param name="request">Refresh token data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>New authentication result with JWT token</returns>
        [HttpPost("refresh")]
        [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RefreshToken(
            [FromBody] RefreshTokenRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest(new ErrorResponse("Request body is required"));

            try
            {
                var command = new RefreshTokenCommand
                {
                    RefreshToken = request.RefreshToken
                };

                var result = await _refreshTokenHandler.Handle(command, cancellationToken);

                var response = new AuthenticationResponse
                {
                    Message = "Token refreshed successfully",
                    AccessToken = result.AccessToken,
                    RefreshToken = null,
                    ExpiresAt = result.ExpiresAt,
                    TokenType = "Bearer"
                };

                return Ok(response);
            }
            catch (ValidationException ex)
            {
                var response = new ValidationErrorResponse("Validation failed")
                {
                    Errors = ex.Errors.Select(e => e.ErrorMessage).ToList()
                };
                return BadRequest(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new ErrorResponse("Invalid refresh token"));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("expired"))
            {
                return Unauthorized(new ErrorResponse("Refresh token has expired"));
            }
            catch (Exception)
            {
                return StatusCode(500, new ErrorResponse("An error occurred while refreshing the token"));
            }
        }

        /// <summary>
        /// Changes a user's password
        /// </summary>
        /// <param name="request">Password change data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success message</returns>
        [HttpPost("change-password")]
        [ProducesResponseType(typeof(PasswordChangedResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest(new ErrorResponse("Request body is required"));

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

                var response = new PasswordChangedResponse
                {
                    Message = "Password changed successfully"
                };

                return Ok(response);
            }
            catch (ValidationException ex)
            {
                var response = new ValidationErrorResponse("Validation failed")
                {
                    Errors = ex.Errors.Select(e => e.ErrorMessage).ToList()
                };
                return BadRequest(response);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new ErrorResponse("Invalid credentials"));
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new ErrorResponse(ex.Message));
            }
            catch (Exception)
            {
                return StatusCode(500, new ErrorResponse("An error occurred while changing the password"));
            }
        }

        /// <summary>
        /// Logs out a user by invalidating their refresh token
        /// </summary>
        /// <param name="request">Logout request data</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success message</returns>
        [HttpPost("logout")]
        [ProducesResponseType(typeof(LogoutResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Logout(
            [FromBody] LogoutRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest(new ErrorResponse("Request body is required"));

            try
            {
                var command = new LogoutCommand
                {
                    RefreshToken = request.RefreshToken
                };

                await _logoutHandler.Handle(command, cancellationToken);

                var response = new LogoutResponse
                {
                    Message = "Logged out successfully"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during logout");
                return StatusCode(500, new ErrorResponse("An error occurred during logout"));
            }
        }

        /// <summary>
        /// Gets the current user's profile information
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User profile information</returns>
        [HttpGet("profile")]
        [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
        {
            try
            {
                // Check if user is authenticated
                if (!_currentUserService.IsAuthenticated())
                {
                    return Unauthorized(new ErrorResponse("User not authenticated"));
                }

                // Get current user information
                var userAccount = await _currentUserService.GetCurrentUserAsync(cancellationToken);
                if (userAccount == null)
                {
                    return Unauthorized(new ErrorResponse("User account not found"));
                }

                // Get employee information
                var employee = await _currentUserService.GetCurrentEmployeeAsync(cancellationToken);
                if (employee == null)
                {
                    return NotFound(new ErrorResponse("Employee information not found"));
                }

                var response = new UserProfileResponse
                {
                    Id = userAccount.Id,
                    Email = userAccount.UserName,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    IsActive = userAccount.IsActive,
                    LastLoginAt = userAccount.PasswordChangedAt // Using this as a proxy for last login
                };

                _logger.LogInformation("Profile retrieved successfully for user: {UserId}", userAccount.Id);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the profile");
                return StatusCode(500, new ErrorResponse("An error occurred while retrieving the profile"));
            }
        }
    }
}
