using System.Security.Cryptography.X509Certificates;

namespace TaskManager.BusinessLogic.Dtos.Auth;

public class AuthenticationResponse
{
    public string? UserName { get; set; } = string.Empty;

    public string? Email { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;

    public DateTime Expiration { get; set; }

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime RefreshTokenExpiration { get; set; }
}
