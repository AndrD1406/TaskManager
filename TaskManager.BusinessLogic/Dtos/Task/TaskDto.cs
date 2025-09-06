using TaskManager.DataAccess.Enums;

namespace TaskManager.BusinessLogic.Dtos.Task;

public class TaskDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public Status Status { get; set; }

    public Priority Priority { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
