using Moq;
using System.Linq.Expressions;
using TaskManager.BusinessLogic.Dtos.Auth;
using TaskManager.BusinessLogic.Services;
using TaskManager.BusinessLogic.Services.Interfaces;
using TaskManager.DataAccess.Models;
using TaskManager.DataAccess.Repository.Base;

namespace TaskManager.Tests.Services;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IEntityRepository<Guid, User>> userRepository;
    private Mock<IJwtService> jwtService;
    private AuthService service;

    [SetUp]
    public void SetUp()
    {
        userRepository = new Mock<IEntityRepository<Guid, User>>();
        jwtService = new Mock<IJwtService>();
        service = new AuthService(userRepository.Object, jwtService.Object);
    }

    [Test]
    public async Task Register_CreatesUser_AndReturnsToken()
    {
        // Arrange
        var request = new RegisterRequest { UserName = "alice", Email = "alice@example.com", Password = "Abcdef1.", ConfirmPassword = "Abcdef1." };
        userRepository.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<string>())).ReturnsAsync(Enumerable.Empty<User>());
        userRepository.Setup(r => r.Create(It.IsAny<User>())).ReturnsAsync((User u) => u);
        jwtService.Setup(j => j.CreateJwtToken(It.IsAny<User>())).Returns(new AuthenticationResponse { UserName = "alice", Email = "alice@example.com", Token = "token" });

        // Act
        var response = await service.RegisterAsync(request);

        // Assert
        Assert.That(response.UserName, Is.EqualTo("alice"));
        Assert.That(response.Email, Is.EqualTo("alice@example.com"));
        Assert.That(response.Token, Is.EqualTo("token"));
        userRepository.Verify(r => r.Create(It.Is<User>(u => u.UserName == "alice" && u.Email == "alice@example.com")), Times.Once);
    }

    [Test]
    public void Register_Throws_WhenWeakPassword()
    {
        // Arrange
        var request = new RegisterRequest { UserName = "u", Email = "u@e.com", Password = "weak", ConfirmPassword = "weak" };

        // Act / Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterAsync(request));
    }

    [Test]
    public void Register_Throws_WhenUsernameTaken()
    {
        // Arrange
        var request = new RegisterRequest { UserName = "dup", Email = "new@e.com", Password = "Abcdef1.", ConfirmPassword = "Abcdef1." };
        userRepository.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<string>())).ReturnsAsync(new List<User> { new User { UserName = "dup" } });

        // Act / Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterAsync(request));
    }

    [Test]
    public void Register_Throws_WhenEmailTaken()
    {
        // Arrange
        var request = new RegisterRequest { UserName = "new", Email = "dup@e.com", Password = "Abcdef1.", ConfirmPassword = "Abcdef1." };
        var sequence = new Queue<IEnumerable<User>>(new[] { Enumerable.Empty<User>(), new List<User> { new User { Email = "dup@e.com" } } });
        userRepository.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<string>())).ReturnsAsync(() => sequence.Dequeue());

        // Act / Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => service.RegisterAsync(request));
    }

    [Test]
    public async Task Login_ReturnsToken_WhenCredentialsValid()
    {
        // Arrange
        var hash = BCrypt.Net.BCrypt.HashPassword("Abcdef1.");
        var user = new User { Id = Guid.NewGuid(), UserName = "alice", Email = "alice@example.com", PasswordHash = hash };
        userRepository.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<string>())).ReturnsAsync(new List<User> { user });
        userRepository.Setup(r => r.Update(It.IsAny<User>())).ReturnsAsync((User u) => u);
        jwtService.Setup(j => j.CreateJwtToken(user)).Returns(new AuthenticationResponse { Token = "token", UserName = "alice", Email = "alice@example.com" });

        // Act
        var response = await service.LoginAsync(new LoginRequest { UserNameOrEmail = "alice", Password = "Abcdef1." });

        // Assert
        Assert.That(response.Token, Is.EqualTo("token"));
        userRepository.Verify(r => r.Update(It.Is<User>(u => u.Id == user.Id)), Times.Once);
    }

    [Test]
    public void Login_Throws_WhenUserNotFound()
    {
        // Arrange
        userRepository.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<string>())).ReturnsAsync(Enumerable.Empty<User>());

        // Act / Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => service.LoginAsync(new LoginRequest { UserNameOrEmail = "ghost", Password = "Abcdef1." }));
    }

    [Test]
    public void Login_Throws_WhenPasswordInvalid()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), UserName = "alice", Email = "alice@example.com", PasswordHash = BCrypt.Net.BCrypt.HashPassword("Right1.") };
        userRepository.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<string>())).ReturnsAsync(new List<User> { user });

        // Act / Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => service.LoginAsync(new LoginRequest { UserNameOrEmail = "alice", Password = "Wrong1." }));
    }
}

