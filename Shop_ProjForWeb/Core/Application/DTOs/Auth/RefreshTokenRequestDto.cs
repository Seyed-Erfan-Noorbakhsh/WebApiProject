namespace Shop_ProjForWeb.Application.DTOs.Auth;

public class RefreshTokenRequestDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
}

