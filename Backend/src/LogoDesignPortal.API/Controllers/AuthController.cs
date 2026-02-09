using LogoDesignPortal.Application.DTOs.Auth;
using LogoDesignPortal.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LogoDesignPortal.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var response = await _authService.RegisterAsync(request);
            return CreatedAtAction(nameof(Login), new { }, response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var response = await _authService.RefreshTokenAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
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

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
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
    public async Task<IActionResult> ResetSuperAdminPassword()
    {
        try
        {
            _logger.LogInformation("Resetting SuperAdmin password to default");
            await _authService.ResetSuperAdminPasswordAsync();
            _logger.LogInformation("SuperAdmin password reset successfully");
            return Ok(new { 
                message = "SuperAdmin password has been reset to default.",
                email = "superadmin@logodesign.com",
                password = "SuperAdmin@123"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting SuperAdmin password");
            return StatusCode(500, new { error = "An error occurred while resetting SuperAdmin password." });
        }
    }

    [HttpPost("forgot-password")]
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
