using System.Security.Claims;
using TaskManager.BusinessLogic.Dtos.Auth;
using TaskManager.DataAccess.Models;

namespace TaskManager.BusinessLogic.Services.Interfaces;

public interface IJwtService
{
    AuthenticationResponse CreateJwtToken(User user);
    ClaimsPrincipal GetPrincipalFromToken(string? token);
}
