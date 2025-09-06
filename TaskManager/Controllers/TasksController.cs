using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskManager.BusinessLogic.Dtos.Task;
using TaskManager.BusinessLogic.Services.Interfaces;
using TaskManager.DataAccess.Models;

namespace TaskManager.Controllers;

[ApiController]
[Route("/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService taskService;
    private readonly ILogger<TasksController> logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        this.taskService = taskService;
        this.logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create([FromBody] TaskCreateUpdateDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var created = await taskService.Create(userId, dto);

        logger.LogInformation("User {UserId} created task {TaskId} with title {Title}",
            userId, created.Id, created.Title);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TaskDto>> GetById(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var task = await taskService.GetById(userId, id);

        if (task == null)
        {
            logger.LogWarning("User {UserId} tried to access non-existing task {TaskId}", userId, id);
            return NotFound();
        }

        logger.LogInformation("User {UserId} retrieved task {TaskId}", userId, id);
        return Ok(task);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TaskDto>> Update(Guid id, [FromBody] TaskCreateUpdateDto dto)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var updated = await taskService.Update(userId, id, dto);

        if (updated == null)
        {
            logger.LogWarning("User {UserId} tried to update non-existing task {TaskId}", userId, id);
            return NotFound();
        }

        logger.LogInformation("User {UserId} updated task {TaskId}", userId, id);
        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var deleted = await taskService.Delete(userId, id);

        if (!deleted)
        {
            logger.LogWarning("User {UserId} tried to delete non-existing task {TaskId}", userId, id);
            return NotFound();
        }

        logger.LogInformation("User {UserId} deleted task {TaskId}", userId, id);
        return NoContent();
    }
}
