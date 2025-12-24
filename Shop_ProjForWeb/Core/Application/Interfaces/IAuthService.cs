using Shop_ProjForWeb.Application.DTOs.Auth;

namespace Shop_ProjForWeb.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress = null);
    Task<bool> RegisterAsync(RegisterRequestDto request);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
    Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string? ipAddress = null);
    Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null);
    Task<bool> VerifyEmailAsync(string token);
    Task<bool> ResendEmailVerificationAsync(string email);
    Task<bool> ForgotPasswordAsync(string email);
    Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request);
    Task<bool> RevokeAllTokensAsync(int userId);
}

