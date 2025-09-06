using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.DataAccess.Enums;

namespace TaskManager.BusinessLogic.Dtos.Task;

public class TaskFilter
{
    public Status? Status { get; set; }
    public Priority? Priority { get; set; }
    public DateTime? DueFrom { get; set; }
    public DateTime? DueTo { get; set; }
}
