using TaskManager.BusinessLogic.Dtos.Auth;

namespace TaskManager.BusinessLogic.Services.Interfaces;

public interface IAuthService
{
    Task<AuthenticationResponse> RegisterAsync(RegisterRequest request);
    Task<AuthenticationResponse> LoginAsync(LoginRequest request);
}
