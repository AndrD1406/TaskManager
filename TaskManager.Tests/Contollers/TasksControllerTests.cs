using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using TaskManager.BusinessLogic.Dtos.Task;
using TaskManager.BusinessLogic.Services.Interfaces;
using TaskManager.Controllers;

namespace TaskManager.Tests.Contollers;

[TestFixture]
public class TasksControllerTests
{
    private static TasksController CreateControllerWithUser(ITaskService taskService, Guid userId)
    {
        var logger = new Mock<Microsoft.Extensions.Logging.ILogger<TasksController>>().Object;
        var controller = new TasksController(taskService, logger);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var user = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        return controller;
    }

    [Test]
    public async Task Create_ReturnsCreatedAt_WithTaskDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskService = new Mock<ITaskService>();
        var dto = new TaskCreateUpdateDto { Title = "Title" };
        var created = new TaskDto { Id = Guid.NewGuid(), Title = "Title" };

        taskService.Setup(s => s.Create(userId, dto)).ReturnsAsync(created);

        var controller = CreateControllerWithUser(taskService.Object, userId);

        // Act
        var result = await controller.Create(dto);

        // Assert
        var createdAt = result.Result as CreatedAtActionResult;
        Assert.IsNotNull(createdAt);
        var payload = createdAt.Value as TaskDto;
        Assert.IsNotNull(payload);
        Assert.That(payload.Id, Is.EqualTo(created.Id));
        Assert.That(payload.Title, Is.EqualTo("Title"));
    }

    [Test]
    public async Task GetById_ReturnsOk_WhenOwned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var taskService = new Mock<ITaskService>();
        var task = new TaskDto { Id = taskId, Title = "Mine" };

        taskService.Setup(s => s.GetById(userId, taskId)).ReturnsAsync(task);

        var controller = CreateControllerWithUser(taskService.Object, userId);

        // Act
        var result = await controller.GetById(taskId);

        // Assert
        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        var payload = ok.Value as TaskDto;
        Assert.IsNotNull(payload);
        Assert.That(payload.Id, Is.EqualTo(taskId));
    }

    [Test]
    public async Task GetById_ReturnsNotFound_WhenMissingOrNotOwned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var taskService = new Mock<ITaskService>();
        taskService.Setup(s => s.GetById(userId, taskId)).ReturnsAsync((TaskDto?)null);

        var controller = CreateControllerWithUser(taskService.Object, userId);

        // Act
        var result = await controller.GetById(taskId);

        // Assert
        Assert.IsInstanceOf<NotFoundResult>(result.Result);
    }

    [Test]
    public async Task Update_ReturnsOk_WhenUpdated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var taskService = new Mock<ITaskService>();
        var input = new TaskCreateUpdateDto { Title = "New" };
        var updated = new TaskDto { Id = taskId, Title = "New" };

        taskService.Setup(s => s.Update(userId, taskId, input)).ReturnsAsync(updated);

        var controller = CreateControllerWithUser(taskService.Object, userId);

        // Act
        var result = await controller.Update(taskId, input);

        // Assert
        var ok = result.Result as OkObjectResult;
        Assert.IsNotNull(ok);
        var payload = ok.Value as TaskDto;
        Assert.IsNotNull(payload);
        Assert.That(payload.Title, Is.EqualTo("New"));
    }

    [Test]
    public async Task Update_ReturnsNotFound_WhenMissingOrNotOwned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var taskService = new Mock<ITaskService>();
        var input = new TaskCreateUpdateDto { Title = "X" };

        taskService.Setup(s => s.Update(userId, taskId, input)).ReturnsAsync((TaskDto?)null);

        var controller = CreateControllerWithUser(taskService.Object, userId);

        // Act
        var result = await controller.Update(taskId, input);

        // Assert
        Assert.IsInstanceOf<NotFoundResult>(result.Result);
    }

    [Test]
    public async Task Delete_ReturnsNoContent_WhenDeleted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var taskService = new Mock<ITaskService>();
        taskService.Setup(s => s.Delete(userId, taskId)).ReturnsAsync(true);

        var controller = CreateControllerWithUser(taskService.Object, userId);

        // Act
        var result = await controller.Delete(taskId);

        // Assert
        Assert.IsInstanceOf<NoContentResult>(result);
    }

    [Test]
    public async Task Delete_ReturnsNotFound_WhenMissingOrNotOwned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var taskService = new Mock<ITaskService>();
        taskService.Setup(s => s.Delete(userId, taskId)).ReturnsAsync(false);

        var controller = CreateControllerWithUser(taskService.Object, userId);

        // Act
        var result = await controller.Delete(taskId);

        // Assert
        Assert.IsInstanceOf<NotFoundResult>(result);
    }
}
