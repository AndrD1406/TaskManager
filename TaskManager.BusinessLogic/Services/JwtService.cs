using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskManager.BusinessLogic.Dtos.Auth;
using TaskManager.BusinessLogic.Services.Interfaces;
using TaskManager.DataAccess.Models;

namespace TaskManager.BusinessLogic.Services;
public class JwtService : IJwtService
{

    //to read Jwt configuration from appsettings.json
    private readonly IConfiguration configuration;
    // size of key must be greater than 256 bites
    private readonly string key;

    public JwtService(IConfiguration config)
    {
        configuration = config;
        key = configuration["Jwt:Key"];
    }

    /// <summary>
    /// Creates a new AuthenticationResponse DTO.
    /// </summary>
    /// <param name="user">The user we want to create the token for.</param>
    /// <returns>A new authentication response.</returns>
    public AuthenticationResponse CreateJwtToken(User user)
    {
        var accessExpiration = DateTime.UtcNow
            .AddMinutes(Convert.ToDouble(configuration["Jwt:AccessTokenExpirationMinutes"]));
        var refreshExpiration = DateTime.UtcNow
            .AddMinutes(Convert.ToDouble(configuration["Jwt:RefreshTokenExpirationMinutes"]));

        var claims = new[]
        {
        new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()),
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Name, user.UserName),
        new Claim(ClaimTypes.Email, user.Email),
    };

        var keyBytes = Convert.FromBase64String(key);
        var securityKey = new SymmetricSecurityKey(keyBytes);
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: configuration["Jwt:Issuer"],
            audience: configuration["Jwt:Audience"],
            claims: claims,
            expires: accessExpiration,
            signingCredentials: signingCredentials
        );

        var encodedToken = new JwtSecurityTokenHandler().WriteToken(token);

        return new AuthenticationResponse
        {
            Token = encodedToken,
            Email = user.Email,
            UserName = user.UserName,
            Expiration = accessExpiration,
            RefreshToken = GenerateRefreshToken(),
            RefreshTokenExpiration = refreshExpiration
        };
    }

    public ClaimsPrincipal? GetPrincipalFromJwtToken(string? token)
    {
        var tokenValidationParameters = new TokenValidationParameters()
        {
            ValidateActor = true,
            ValidAudience = configuration["Jwt:Audience"],
            ValidateIssuer = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(configuration["Jwt:Key"])),
            ValidateLifetime = false,
        };

        var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
        ClaimsPrincipal principal = null;

        principal = jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }

    public ClaimsPrincipal GetPrincipalFromToken(string? token)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Creates a refresh token (base 64 string of random numbers)
    /// </summary>
    /// <returns></returns>
    private string GenerateRefreshToken()
    {
        byte[] bytes = new byte[64];
        var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
