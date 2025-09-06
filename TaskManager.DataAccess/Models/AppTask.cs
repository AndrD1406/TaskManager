using TaskManager.DataAccess.Enums;

namespace TaskManager.DataAccess.Models;

public class AppTask : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public Status Status { get; set; }

    public Priority Priority { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    public Guid UserId { get; set; }

    public User User { get; set; }

}
