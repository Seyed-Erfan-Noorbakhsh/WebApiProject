using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Shop_ProjForWeb.Application.DTOs.Auth;
using Shop_ProjForWeb.Application.Interfaces;
using Shop_ProjForWeb.Core.Domain.Entities;
using Shop_ProjForWeb.Core.Application.DTOs.Auth;
using Shop_ProjForWeb.Domain.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using Shop_ProjForWeb.Core.Application.DTOs.User;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly ILoggerService _logger;

    public AuthService(
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        IMapper mapper,
        ILoggerService logger)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string? ipAddress = null)
    {
        _logger.LogInformation("Login attempt for username: {Username} from IP: {IpAddress}", request.Username, ipAddress);

        var user = (await _unitOfWork.Users.FindAsync(u => u.Username == request.Username && !u.IsDeleted))
            .FirstOrDefault();

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Login failed for username: {Username} from IP: {IpAddress}", request.Username, ipAddress);
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt for inactive user: {Username}", request.Username);
            throw new UnauthorizedAccessException("User account is inactive");
        }

        // Revoke all existing refresh tokens for security
        await RevokeAllTokensAsync(user.Id);

        user.LastLoginAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);

        var token = GenerateJwtToken(user);
        var refreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id, ipAddress);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("User {Username} logged in successfully from IP: {IpAddress}", user.Username, ipAddress);

        return new LoginResponseDto
        {
            Token = token,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddHours(1),
            User = await MapUserToDto(user)
        };
    }

    public async Task<bool> RegisterAsync(RegisterRequestDto request)
    {
        _logger.LogInformation("Registration attempt for username: {Username}, email: {Email}", request.Username, request.Email);

        if (request.Password != request.ConfirmPassword)
        {
            throw new ArgumentException("Passwords do not match");
        }

        if (request.Password.Length < 8)
        {
            throw new ArgumentException("Password must be at least 8 characters long");
        }

        var existingUser = (await _unitOfWork.Users.FindAsync(u => 
            u.Username == request.Username || u.Email == request.Email)).FirstOrDefault();

        if (existingUser != null)
        {
            throw new InvalidOperationException("Username or email already exists");
        }

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = true,
            EmailConfirmed = false,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user);
        await _unitOfWork.SaveChangesAsync();

        // Generate email verification token
        await GenerateEmailVerificationTokenAsync(user.Id);

        _logger.LogInformation("User {Username} registered successfully. Email verification token generated.", user.Username);

        return true;
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        _logger.LogInformation("Password change attempt for user ID: {UserId}", userId);

        if (newPassword.Length < 8)
        {
            throw new ArgumentException("Password must be at least 8 characters long");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            _logger.LogWarning("Password change failed - incorrect current password for user ID: {UserId}", userId);
            throw new UnauthorizedAccessException("Current password is incorrect");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);

        // Revoke all tokens after password change
        await RevokeAllTokensAsync(userId);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Password changed successfully for user ID: {UserId}", userId);
        return true;
    }

    public async Task<RefreshTokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request, string? ipAddress = null)
    {
        _logger.LogInformation("Refresh token request from IP: {IpAddress}", ipAddress);

        var refreshToken = (await _unitOfWork.RefreshTokens.FindAsync(rt => 
            rt.Token == request.RefreshToken && !rt.IsRevoked && !rt.IsDeleted))
            .FirstOrDefault();

        if (refreshToken == null || refreshToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Invalid or expired refresh token from IP: {IpAddress}", ipAddress);
            throw new UnauthorizedAccessException("Invalid or expired refresh token");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(refreshToken.UserId);
        if (user == null || !user.IsActive)
        {
            _logger.LogWarning("User not found or inactive for refresh token");
            throw new UnauthorizedAccessException("User not found or inactive");
        }

        // Revoke old refresh token
        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        await _unitOfWork.RefreshTokens.UpdateAsync(refreshToken);

        // Generate new tokens
        var newToken = GenerateJwtToken(user);
        var newRefreshToken = await GenerateAndSaveRefreshTokenAsync(user.Id, ipAddress);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Token refreshed successfully for user ID: {UserId}", user.Id);

        return new RefreshTokenResponseDto
        {
            Token = newToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken, string? ipAddress = null)
    {
        var token = (await _unitOfWork.RefreshTokens.FindAsync(rt => 
            rt.Token == refreshToken && !rt.IsRevoked && !rt.IsDeleted))
            .FirstOrDefault();

        if (token == null)
        {
            return false;
        }

        token.IsRevoked = true;
        token.RevokedAt = DateTime.UtcNow;
        token.RevokedByIp = ipAddress;
        await _unitOfWork.RefreshTokens.UpdateAsync(token);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Refresh token revoked for user ID: {UserId} from IP: {IpAddress}", token.UserId, ipAddress);
        return true;
    }

    public async Task<bool> RevokeAllTokensAsync(int userId)
    {
        var tokens = await _unitOfWork.RefreshTokens.FindAsync(rt => 
            rt.UserId == userId && !rt.IsRevoked && !rt.IsDeleted);

        foreach (var token in tokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            await _unitOfWork.RefreshTokens.UpdateAsync(token);
        }

        await _unitOfWork.SaveChangesAsync();
        _logger.LogInformation("All refresh tokens revoked for user ID: {UserId}", userId);
        return true;
    }

    public async Task<bool> VerifyEmailAsync(string token)
    {
        _logger.LogInformation("Email verification attempt with token");

        var verificationToken = (await _unitOfWork.EmailVerificationTokens.FindAsync(evt => 
            evt.Token == token && !evt.IsUsed && !evt.IsDeleted))
            .FirstOrDefault();

        if (verificationToken == null || verificationToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Invalid or expired email verification token");
            throw new UnauthorizedAccessException("Invalid or expired verification token");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(verificationToken.UserId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found");
        }

        user.EmailConfirmed = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);

        verificationToken.IsUsed = true;
        verificationToken.UsedAt = DateTime.UtcNow;
        await _unitOfWork.EmailVerificationTokens.UpdateAsync(verificationToken);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Email verified successfully for user ID: {UserId}", user.Id);
        return true;
    }

    public async Task<bool> ResendEmailVerificationAsync(string email)
    {
        var user = (await _unitOfWork.Users.FindAsync(u => u.Email == email && !u.IsDeleted))
            .FirstOrDefault();

        if (user == null)
        {
            // Don't reveal if user exists or not for security
            return true;
        }

        if (user.EmailConfirmed)
        {
            throw new InvalidOperationException("Email is already verified");
        }

        // Revoke old tokens
        var oldTokens = await _unitOfWork.EmailVerificationTokens.FindAsync(evt => 
            evt.UserId == user.Id && !evt.IsUsed && !evt.IsDeleted);

        foreach (var token in oldTokens)
        {
            token.IsUsed = true;
            await _unitOfWork.EmailVerificationTokens.UpdateAsync(token);
        }

        await GenerateEmailVerificationTokenAsync(user.Id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Email verification token resent for user ID: {UserId}", user.Id);
        return true;
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        _logger.LogInformation("Password reset request for email: {Email}", email);

        var user = (await _unitOfWork.Users.FindAsync(u => u.Email == email && !u.IsDeleted))
            .FirstOrDefault();

        if (user == null)
        {
            // Don't reveal if user exists or not for security
            return true;
        }

        // Revoke old password reset tokens
        var oldTokens = await _unitOfWork.PasswordResetTokens.FindAsync(prt => 
            prt.UserId == user.Id && !prt.IsUsed && !prt.IsDeleted);

        foreach (var token in oldTokens)
        {
            token.IsUsed = true;
            await _unitOfWork.PasswordResetTokens.UpdateAsync(token);
        }

        await GeneratePasswordResetTokenAsync(user.Id);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Password reset token generated for user ID: {UserId}", user.Id);
        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequestDto request)
    {
        _logger.LogInformation("Password reset attempt");

        if (request.NewPassword != request.ConfirmPassword)
        {
            throw new ArgumentException("Passwords do not match");
        }

        if (request.NewPassword.Length < 8)
        {
            throw new ArgumentException("Password must be at least 8 characters long");
        }

        var resetToken = (await _unitOfWork.PasswordResetTokens.FindAsync(prt => 
            prt.Token == request.Token && !prt.IsUsed && !prt.IsDeleted))
            .FirstOrDefault();

        if (resetToken == null || resetToken.ExpiresAt < DateTime.UtcNow)
        {
            _logger.LogWarning("Invalid or expired password reset token");
            throw new UnauthorizedAccessException("Invalid or expired reset token");
        }

        var user = await _unitOfWork.Users.GetByIdAsync(resetToken.UserId);
        if (user == null || user.Email != request.Email)
        {
            throw new UnauthorizedAccessException("Invalid reset token or email");
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Users.UpdateAsync(user);

        // Revoke all tokens after password reset
        await RevokeAllTokensAsync(user.Id);

        resetToken.IsUsed = true;
        resetToken.UsedAt = DateTime.UtcNow;
        await _unitOfWork.PasswordResetTokens.UpdateAsync(resetToken);

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Password reset successfully for user ID: {UserId}", user.Id);
        return true;
    }

    private string GenerateJwtToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new("email_confirmed", user.EmailConfirmed.ToString())
        };

        // Add roles
        var userRoles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        foreach (var role in userRoles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // Add permissions
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => $"{rp.Permission.Resource}.{rp.Permission.Action}")
            .Distinct()
            .ToList();

        foreach (var permission in permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationInMinutes"] ?? "60");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            _configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT Key not configured")));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private async Task<RefreshToken> GenerateAndSaveRefreshTokenAsync(int userId, string? ipAddress)
    {
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7), // Refresh token valid for 7 days
            CreatedByIp = ipAddress,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
        return refreshToken;
    }

    private async Task GenerateEmailVerificationTokenAsync(int userId)
    {
        var token = new EmailVerificationToken
        {
            UserId = userId,
            Token = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7), // Valid for 7 days
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.EmailVerificationTokens.AddAsync(token);
    }

    private async Task GeneratePasswordResetTokenAsync(int userId)
    {
        var token = new PasswordResetToken
        {
            UserId = userId,
            Token = GenerateSecureToken(),
            ExpiresAt = DateTime.UtcNow.AddHours(24), // Valid for 24 hours
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.PasswordResetTokens.AddAsync(token);
    }

    private string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").Replace("=", "");
    }

    private async Task<UserDto> MapUserToDto(User user)
    {
        var roles = user.UserRoles.Select(ur => ur.Role.Name).ToList();
        var permissions = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => $"{rp.Permission.Resource}.{rp.Permission.Action}")
            .Distinct()
            .ToList();

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            LastLoginAt = user.LastLoginAt,
            Roles = roles,
            Permissions = permissions
        };
    }
}
