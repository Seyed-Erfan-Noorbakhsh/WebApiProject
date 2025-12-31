using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop_ProjForWeb.Application.DTOs.Auth;
using Shop_ProjForWeb.Application.Interfaces;

namespace Shop_ProjForWeb.API.Controllers;

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
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var ipAddress = GetClientIpAddress();
            var response = await _authService.LoginAsync(request, ipAddress);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, new { message = "An error occurred during login" });
        }
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            if (result)
            {
                return Ok(new { message = "User registered successfully. Please check your email for verification." });
            }
            return BadRequest(new { message = "Registration failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        try
        {
            var ipAddress = GetClientIpAddress();
            var response = await _authService.RefreshTokenAsync(request, ipAddress);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { message = "An error occurred during token refresh" });
        }
    }

    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenRequestDto request)
    {
        try
        {
            var ipAddress = GetClientIpAddress();
            var result = await _authService.RevokeTokenAsync(request.RefreshToken, ipAddress);
            if (result)
            {
                return Ok(new { message = "Token revoked successfully" });
            }
            return BadRequest(new { message = "Token revocation failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token revocation");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("revoke-all-tokens")]
    [Authorize]
    public async Task<IActionResult> RevokeAllTokens()
    {
        try
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _authService.RevokeAllTokensAsync(userId);
            if (result)
            {
                return Ok(new { message = "All tokens revoked successfully" });
            }
            return BadRequest(new { message = "Token revocation failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token revocation");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromBody] EmailVerificationRequestDto request)
    {
        try
        {
            var result = await _authService.VerifyEmailAsync(request.Token);
            if (result)
            {
                return Ok(new { message = "Email verified successfully" });
            }
            return BadRequest(new { message = "Email verification failed" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("resend-email-verification")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendEmailVerification([FromBody] ResendEmailVerificationDto request)
    {
        try
        {
            var result = await _authService.ResendEmailVerificationAsync(request.Email);
            return Ok(new { message = "If the email exists and is not verified, a verification email has been sent." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during resend email verification");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        try
        {
            var result = await _authService.ForgotPasswordAsync(request.Email);
            return Ok(new { message = "If the email exists, a password reset link has been sent." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during forgot password");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        try
        {
            var result = await _authService.ResetPasswordAsync(request);
            if (result)
            {
                return Ok(new { message = "Password reset successfully" });
            }
            return BadRequest(new { message = "Password reset failed" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset");
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
    {
        try
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
            var result = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
            if (result)
            {
                return Ok(new { message = "Password changed successfully" });
            }
            return BadRequest(new { message = "Password change failed" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password change");
            return BadRequest(new { message = ex.Message });
        }
    }

    private string? GetClientIpAddress()
    {
        var ipAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ipAddress))
        {
            ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        }
        return ipAddress;
    }
}

public class ChangePasswordRequestDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class RevokeTokenRequestDto
{
    public string RefreshToken { get; set; } = string.Empty;
}
