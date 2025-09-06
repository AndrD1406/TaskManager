using System.Text.RegularExpressions;
using TaskManager.BusinessLogic.Dtos.Auth;
using TaskManager.BusinessLogic.Services.Interfaces;
using TaskManager.DataAccess.Models;
using TaskManager.DataAccess.Repository.Base;

namespace TaskManager.BusinessLogic.Services;

public class AuthService : IAuthService
{
    private readonly IEntityRepository<Guid, User> userRepository;
    private readonly IJwtService jwtService;

    public AuthService(IEntityRepository<Guid, User> users, IJwtService jwt)
    {
        this.userRepository = users;
        this.jwtService = jwt;
    }

    public async Task<AuthenticationResponse> RegisterAsync(RegisterRequest request)
    {
        if (!IsPasswordStrong(request.Password)) throw new InvalidOperationException("Weak password");
        var existsByName = (await userRepository.GetByFilter(x => x.UserName == request.UserName)).Any();
        if (existsByName) throw new InvalidOperationException("Username already used");
        var existsByEmail = (await userRepository.GetByFilter(x => x.Email == request.Email)).Any();
        if (existsByEmail) throw new InvalidOperationException("Email already used");

        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await userRepository.Create(user);
        return jwtService.CreateJwtToken(user);
    }

    public async Task<AuthenticationResponse> LoginAsync(LoginRequest request)
    {
        var found = await userRepository.GetByFilter(x => x.UserName == request.UserNameOrEmail || x.Email == request.UserNameOrEmail);
        var user = found.FirstOrDefault();
        if (user == null) throw new InvalidOperationException("Invalid credentials");
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash)) throw new InvalidOperationException("Invalid credentials");
        user.UpdatedAt = DateTime.UtcNow;
        await userRepository.Update(user);
        return jwtService.CreateJwtToken(user);
    }

    private static bool IsPasswordStrong(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;
        if (password.Length < 8) return false;
        var hasUpper = Regex.IsMatch(password, "[A-Z]");
        var hasLower = Regex.IsMatch(password, "[a-z]");
        var hasDigit = Regex.IsMatch(password, "[0-9]");
        var hasSpecialChars = Regex.IsMatch(password, "[()_.-]");
        return hasUpper && hasLower && hasDigit && hasSpecialChars;
    }
}
