using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.DataAccess.Enums;

namespace TaskManager.BusinessLogic.Dtos.Task;

public class TasksQueryDto
{
    public Status? Status { get; set; }
    public Priority? Priority { get; set; }
    public DateTime? DueFrom { get; set; }
    public DateTime? DueTo { get; set; }
    public string? SortBy { get; set; }
    public SortDirection Direction { get; set; } = SortDirection.Ascending;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}
