using TaskManager.DataAccess.Enums;

namespace TaskManager.BusinessLogic.Dtos.Task;

public class TaskCreateUpdateDto
{
    public string Title { get; set; }

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public Status Status { get; set; }

    public Priority Priority { get; set; }
}
