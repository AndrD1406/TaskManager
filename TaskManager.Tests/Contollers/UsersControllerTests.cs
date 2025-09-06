using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.BusinessLogic.Dtos.Auth;
using TaskManager.BusinessLogic.Services.Interfaces;
using TaskManager.Controllers;

namespace TaskManager.Tests.Contollers;

[TestFixture]
public class UsersControllerTests
{
    [Test]
    public async Task Register_ReturnsOk_WithToken()
    {
        // Arrange
        var authService = new Mock<IAuthService>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<UsersController>>();
        var controller = new UsersController(authService.Object, logger.Object);

        var request = new RegisterRequest
        {
            UserName = "alice",
            Email = "alice@example.com",
            Password = "P@ssw0rd!",
            ConfirmPassword = "P@ssw0rd!"
        };

        var expected = new AuthenticationResponse
        {
            UserName = "alice",
            Email = "alice@example.com",
            Token = "token"
        };

        authService.Setup(s => s.RegisterAsync(request)).ReturnsAsync(expected);

        // Act
        var result = await controller.Register(request);

        // Assert
        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        var payload = ok.Value as AuthenticationResponse;
        Assert.IsNotNull(payload);
        Assert.That(payload.Token, Is.EqualTo("token"));
    }


    [Test]
    public async Task Login_ReturnsOk_WithToken()
    {
        // Arrange
        var authService = new Mock<IAuthService>();
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<UsersController>>();
        var controller = new UsersController(authService.Object, logger.Object);

        var request = new LoginRequest { UserNameOrEmail = "alice", Password = "P@ssw0rd!" };
        var expected = new AuthenticationResponse { UserName = "alice", Email = "alice@example.com", Token = "token" };

        authService.Setup(s => s.LoginAsync(request)).ReturnsAsync(expected);

        // Act
        var result = await controller.Login(request);

        // Assert
        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        var payload = ok.Value as AuthenticationResponse;
        Assert.IsNotNull(payload);
        Assert.That(payload.Token, Is.EqualTo("token"));
    }
}
