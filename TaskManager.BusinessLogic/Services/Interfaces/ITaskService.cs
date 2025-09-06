using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager.BusinessLogic.Dtos.Task;
using TaskManager.DataAccess.Models;

namespace TaskManager.BusinessLogic.Services.Interfaces;

public interface ITaskService
{
    Task<TaskDto> Create(Guid userId, TaskCreateUpdateDto dto);
    Task<PagedResult<TaskDto>> Get(Guid userId, TasksQueryDto query);
    Task<TaskDto?> GetById(Guid userId, Guid taskId);
    Task<TaskDto?> Update(Guid userId, Guid taskId, TaskCreateUpdateDto dto);
    Task<bool> Delete(Guid userId, Guid taskId);
}
