using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.BusinessLogic.Dtos.Auth;
using TaskManager.BusinessLogic.Services.Interfaces;

namespace TaskManager.Controllers;

[Route("/[controller]/[action]")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly IAuthService authService;
    private readonly ILogger<UsersController> logger;

    public UsersController(IAuthService authService, ILogger<UsersController> logger)
    {
        this.authService = authService;
        this.logger = logger;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<AuthenticationResponse>> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var result = await authService.RegisterAsync(request);
        logger.LogInformation("User {UserName} registered with email {Email}", request.UserName, request.Email);

        return Ok(result);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<AuthenticationResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var result = await authService.LoginAsync(request);
        logger.LogInformation("User {UserNameOrEmail} logged in successfully", request.UserNameOrEmail);

        return Ok(result);
    }
}
