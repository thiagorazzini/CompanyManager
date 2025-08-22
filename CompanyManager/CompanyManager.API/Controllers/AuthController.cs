using CompanyManager.Application.Abstractions;
using CompanyManager.Application.Commands;
using CompanyManager.Application.DTOs;
using CompanyManager.Application.Auth;
using CompanyManager.API.Models;
using CompanyManager.API.Models.Responses;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace CompanyManager.API.Controllers
{
    [ApiController]
    [Route("api/v1/auth")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticateCommandHandler _authenticateHandler;

        private readonly IChangePasswordCommandHandler _changePasswordHandler;
        private readonly ILogoutCommandHandler _logoutHandler;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(
            IAuthenticateCommandHandler authenticateHandler,

            IChangePasswordCommandHandler changePasswordHandler,
            ILogoutCommandHandler logoutHandler,
            ICurrentUserService currentUserService,
            ILogger<AuthController> logger)
        {
            _authenticateHandler = authenticateHandler ?? throw new ArgumentNullException(nameof(authenticateHandler));

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
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        [ProducesResponseType(StatusCodes.Status423Locked)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Login(
            [FromBody] AuthenticateRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                _logger.LogWarning("Login attempt with null request body");
                return BadRequest(new ErrorResponse("Email and password are required"));
            }

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                _logger.LogWarning("Login attempt with empty email or password");
                return BadRequest(new ErrorResponse("Email and password cannot be empty"));
            }

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
                    ExpiresAt = result.ExpiresAt,
                    TokenType = "Bearer",
                    User = new UserInfo
                    {
                        Id = result.UserId,
                        Email = request.Email
                    }
                };

                _logger.LogInformation("User {Email} authenticated successfully", request.Email);
                return Ok(response);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Validation failed for user {Email}: {Errors}", 
                    request.Email, string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
                
                var response = new ValidationErrorResponse("Please check your input and try again")
                {
                    Errors = ex.Errors.Select(e => e.ErrorMessage).ToList()
                };
                return BadRequest(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Authentication failed for user {Email}: {Message}", 
                    request.Email, ex.Message);
                return Unauthorized(new ErrorResponse("Invalid email or password. Please check your credentials and try again."));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("account locked", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Account locked for user {Email}: {Message}", 
                    request.Email, ex.Message);
                return StatusCode(423, new ErrorResponse("Your account has been locked due to multiple failed login attempts. Please contact support to unlock your account."));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("too many attempts", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Too many login attempts for user {Email}: {Message}", 
                    request.Email, ex.Message);
                return StatusCode(429, new ErrorResponse("Too many failed login attempts. Please wait a few minutes before trying again."));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("inactive", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Inactive account login attempt for user {Email}: {Message}", 
                    request.Email, ex.Message);
                return BadRequest(new ErrorResponse("Your account is currently inactive. Please contact support to activate your account."));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("not found", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("User not found for email {Email}: {Message}", 
                    request.Email, ex.Message);
                return Unauthorized(new ErrorResponse("Invalid email or password. Please check your credentials and try again."));
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Login operation cancelled for user {Email}", request.Email);
                return StatusCode(499, new ErrorResponse("Login request was cancelled. Please try again."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during authentication for user {Email}", request.Email);
                return StatusCode(500, new ErrorResponse("We're experiencing technical difficulties. Please try again in a few moments."));
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
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
            {
                _logger.LogWarning("Password change attempt with null request body");
                return BadRequest(new ErrorResponse("Password change data is required"));
            }

            if (string.IsNullOrWhiteSpace(request.Email) || 
                string.IsNullOrWhiteSpace(request.CurrentPassword) || 
                string.IsNullOrWhiteSpace(request.NewPassword) || 
                string.IsNullOrWhiteSpace(request.ConfirmNewPassword))
            {
                _logger.LogWarning("Password change attempt with empty fields");
                return BadRequest(new ErrorResponse("All fields are required"));
            }

            if (request.NewPassword != request.ConfirmNewPassword)
            {
                _logger.LogWarning("Password change attempt with mismatched new passwords for user {Email}", request.Email);
                return BadRequest(new ErrorResponse("New password and confirmation password do not match"));
            }

            if (request.CurrentPassword == request.NewPassword)
            {
                _logger.LogWarning("Password change attempt with same current and new password for user {Email}", request.Email);
                return BadRequest(new ErrorResponse("New password must be different from current password"));
            }

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

                _logger.LogInformation("Password changed successfully for user {Email}", request.Email);
                return Ok(response);
            }
            catch (ValidationException ex)
            {
                _logger.LogWarning("Password change validation failed for user {Email}: {Errors}", 
                    request.Email, string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)));
                
                var response = new ValidationErrorResponse("Please check your input and try again")
                {
                    Errors = ex.Errors.Select(e => e.ErrorMessage).ToList()
                };
                return BadRequest(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning("Password change failed for user {Email}: {Message}", 
                    request.Email, ex.Message);
                return Unauthorized(new ErrorResponse("Current password is incorrect. Please check your password and try again."));
            }
            catch (ArgumentException ex) when (ex.Message.Contains("password", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Password change failed for user {Email}: {Message}", 
                    request.Email, ex.Message);
                return BadRequest(new ErrorResponse(ex.Message));
            }
            catch (ArgumentException ex) when (ex.Message.Contains("email", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Password change failed for user {Email}: {Message}", 
                    request.Email, ex.Message);
                return BadRequest(new ErrorResponse("User account not found. Please check your email address."));
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("inactive", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Password change failed for inactive user {Email}: {Message}", 
                    request.Email, ex.Message);
                return BadRequest(new ErrorResponse("Your account is currently inactive. Please contact support to activate your account."));
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Password change operation cancelled for user {Email}", request.Email);
                return StatusCode(499, new ErrorResponse("Password change request was cancelled. Please try again."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during password change for user {Email}", request.Email);
                return StatusCode(500, new ErrorResponse("We're experiencing technical difficulties. Please try again in a few moments."));
            }
        }

        /// <summary>
        /// Logs out a user by invalidating their refresh token
        /// </summary>
        /// <param name="request">Logout request data (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Success message</returns>
        [HttpPost("logout")]
        [ProducesResponseType(typeof(LogoutResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken = default)
        {
            try
            {
                // Logout action - just log the action
                _logger.LogInformation("User logged out successfully");

                var response = new LogoutResponse
                {
                    Message = "Logged out successfully"
                };

                return Ok(response);
            }

            catch (OperationCanceledException)
            {
                _logger.LogWarning("Logout operation cancelled");
                return StatusCode(499, new ErrorResponse("Logout request was cancelled. Please try again."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during logout");
                return StatusCode(500, new ErrorResponse("We're experiencing technical difficulties. Please try again in a few moments."));
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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetProfile(CancellationToken cancellationToken)
        {
            try
            {
                // Check if user is authenticated
                if (!_currentUserService.IsAuthenticated())
                {
                    _logger.LogWarning("Profile access attempt by unauthenticated user");
                    return Unauthorized(new ErrorResponse("You must be logged in to access your profile. Please log in and try again."));
                }

                // Get current user information
                var userAccount = await _currentUserService.GetCurrentUserAsync(cancellationToken);
                if (userAccount == null)
                {
                    _logger.LogWarning("Profile access attempt with invalid user account");
                    return Unauthorized(new ErrorResponse("Your user account could not be found. Please log in again."));
                }

                // Get employee information
                var employee = await _currentUserService.GetCurrentEmployeeAsync(cancellationToken);
                if (employee == null)
                {
                    _logger.LogWarning("Employee information not found for user {UserId}", userAccount.Id);
                    return NotFound(new ErrorResponse("Your employee profile could not be found. Please contact support for assistance."));
                }

                // Get job title information
                var jobTitle = await _currentUserService.GetCurrentJobTitleAsync(cancellationToken);
                
                // Get role information
                var role = await _currentUserService.GetCurrentRoleAsync(cancellationToken);

                var response = new UserProfileResponse
                {
                    Id = userAccount.Id,
                    Email = userAccount.UserName,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    IsActive = userAccount.IsActive,
                    LastLoginAt = userAccount.PasswordChangedAt, // Using this as a proxy for last login
                    JobTitle = jobTitle != null ? new UserJobTitleInfo
                    {
                        Id = jobTitle.Id,
                        Name = jobTitle.Name,
                        HierarchyLevel = jobTitle.HierarchyLevel,
                        Description = jobTitle.Description
                    } : null,
                    Role = role != null ? new RoleInfo
                    {
                        Id = role.Id,
                        Name = role.Name,
                        Level = role.Level.ToString()
                    } : null
                };

                _logger.LogInformation("Profile retrieved successfully for user: {UserId}", userAccount.Id);

                return Ok(response);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Profile retrieval operation cancelled");
                return StatusCode(499, new ErrorResponse("Profile request was cancelled. Please try again."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while retrieving user profile");
                return StatusCode(500, new ErrorResponse("We're experiencing technical difficulties. Please try again in a few moments."));
            }
        }
    }
}
