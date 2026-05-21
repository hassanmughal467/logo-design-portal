using LogoDesignPortal.API.Extensions;
using LogoDesignPortal.API.Services;
using LogoDesignPortal.Application.DTOs.Auth;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IAuthCookieService _authCookies;
    private readonly ILogger<AuthController> _logger;
    private readonly IWebHostEnvironment _environment;

    public AuthController(IAuthService authService, IAuthCookieService authCookies, ILogger<AuthController> logger, IWebHostEnvironment environment)
    {
        _authService = authService;
        _authCookies = authCookies;
        _logger = logger;
        _environment = environment;
    }

    private IActionResult AuthSuccess(AuthResponseDto response, bool created = false)
    {
        _authCookies.SetAuthCookies(Response, response);
        if (created)
            return CreatedAtAction(nameof(Login), new { }, response);
        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return AuthSuccess(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(this.StandardError(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for email: {Email}. Error: {Message}", request?.Email, ex.Message);
            return StatusCode(500, this.StandardError("An error occurred while processing your request. Please ensure the database is running and migrations have been applied."));
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (request == null)
        {
            return BadRequest(this.StandardError("Request body is required."));
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                .ToList();
            return BadRequest(this.StandardError(string.Join(" ", errors)));
        }

        try
        {
            var response = await _authService.RegisterAsync(request);
            return AuthSuccess(response, created: true);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(this.StandardError(ex.Message));
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex)
        {
            _logger.LogError(ex, "Registration database error for email: {Email}. Inner: {Inner}", request.Email, ex.InnerException?.Message);
            var innerMsg = ex.InnerException?.Message ?? ex.Message;
            var isConstraintOrSchema = innerMsg.Contains("Duplicate") || innerMsg.Contains("column") || innerMsg.Contains("migration");
            return StatusCode(500, this.StandardError(isConstraintOrSchema
                ? "Registration failed. The email may already exist, or the database may need migrations applied."
                : "Registration failed. Please try again."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Registration failed for email: {Email}. Error: {Message}", request.Email, ex.Message);
            return StatusCode(500, this.StandardError("Registration failed. Please try again."));
        }
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto? request)
    {
        try
        {
            request ??= new RefreshTokenRequestDto();
            if (string.IsNullOrWhiteSpace(request.RefreshToken)
                && Request.Cookies.TryGetValue("ldp_refresh", out var refreshFromCookie))
            {
                request.RefreshToken = refreshFromCookie;
            }

            if (string.IsNullOrWhiteSpace(request.Token)
                && Request.Cookies.TryGetValue("ldp_access", out var accessFromCookie))
            {
                request.Token = accessFromCookie;
            }

            var response = await _authService.RefreshTokenAsync(request);
            return AuthSuccess(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(this.StandardError(ex.Message));
        }
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        _authCookies.ClearAuthCookies(Response);
        return Ok(new { message = "Logged out." });
    }

    [HttpPost("change-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        try
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();
                return BadRequest(new { error = string.Join(" ", errors) });
            }

            var userId = User.GetUserIdOrThrow();
            _logger.LogInformation("Changing password for user: {UserId}", userId);
            await _authService.ChangePasswordAsync(userId, request);
            _logger.LogInformation("Password changed successfully for user: {UserId}", userId);
            return Ok(new { message = "Password changed successfully." });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Password change failed - Unauthorized: {Message}", ex.Message);
            return Unauthorized(new { error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Password change failed - Invalid operation: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during password change");
            return StatusCode(500, new { error = "An unexpected error occurred while changing password." });
        }
    }

    [HttpPost("reset-password")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        try
        {
            _logger.LogInformation("Resetting password for user: {UserId}", request.UserId);
            await _authService.ResetPasswordAsync(request.UserId, request);
            _logger.LogInformation("Password reset successfully for user: {UserId}", request.UserId);
            return Ok(new { message = "Password reset successfully." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Password reset failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during password reset");
            return StatusCode(500, new { error = "An unexpected error occurred while resetting password." });
        }
    }

    [HttpPost("reset-superadmin-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ResetSuperAdminPassword()
    {
        try
        {
            // Disable in production - only available in Development for initial setup/recovery
            if (!_environment.IsDevelopment())
            {
                _logger.LogWarning("ResetSuperAdminPassword rejected - endpoint disabled in non-Development environment");
                return StatusCode(StatusCodes.Status404NotFound, new { error = "This endpoint is not available." });
            }
            // Restrict to localhost only for security - prevents remote password reset attacks
            var remoteIp = HttpContext.Connection.RemoteIpAddress;
            if (remoteIp != null && !System.Net.IPAddress.IsLoopback(remoteIp))
            {
                _logger.LogWarning("ResetSuperAdminPassword rejected - request from non-localhost: {RemoteIp}", remoteIp);
                return StatusCode(StatusCodes.Status403Forbidden, new { error = "This endpoint is only available from localhost." });
            }

            _logger.LogInformation("Resetting SuperAdmin password to default");
            await _authService.ResetSuperAdminPasswordAsync();
            _logger.LogWarning("SuperAdmin password reset. Dev credentials: superadmin@logodesign.com / SuperAdmin@123 - Check server logs only, never expose in API.");
            return Ok(new { 
                message = "SuperAdmin password has been reset to default. Check server logs for credentials (development only)."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting SuperAdmin password");
            return StatusCode(500, new { error = "An error occurred while resetting SuperAdmin password." });
        }
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        try
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();
                return BadRequest(new { error = string.Join(" ", errors) });
            }

            _logger.LogInformation("Processing forgot password request for email: {Email}", request.Email);
            var response = await _authService.ForgotPasswordAsync(request);
            _logger.LogInformation("Password reset link sent successfully for email: {Email}", request.Email);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Forgot password failed - User not found: {Email}", request.Email);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password request");
            return StatusCode(500, new { error = "An error occurred while processing your request. Please try again later." });
        }
    }

    [HttpPost("reset-password-with-token")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPasswordWithToken([FromBody] ResetPasswordWithTokenRequestDto request)
    {
        try
        {
            // Validate model state
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value?.Errors.Count > 0)
                    .SelectMany(x => x.Value!.Errors.Select(e => e.ErrorMessage))
                    .ToList();
                return BadRequest(new { error = string.Join(" ", errors) });
            }

            _logger.LogInformation("Resetting password with token for email: {Email}", request.Email);
            await _authService.ResetPasswordWithTokenAsync(request);
            _logger.LogInformation("Password reset successfully with token for email: {Email}", request.Email);
            return Ok(new { message = "Password has been reset successfully. You can now login with your new password." });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Password reset with token failed: {Message}", ex.Message);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during password reset with token");
            return StatusCode(500, new { error = "An unexpected error occurred while resetting password." });
        }
    }
}
