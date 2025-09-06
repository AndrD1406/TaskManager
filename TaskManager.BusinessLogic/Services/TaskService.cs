using AutoMapper;
using System.Linq.Expressions;
using TaskManager.BusinessLogic.Dtos.Task;
using TaskManager.BusinessLogic.Services.Interfaces;
using TaskManager.DataAccess.Enums;
using TaskManager.DataAccess.Models;
using TaskManager.DataAccess.Repository.Base;

namespace TaskManager.BusinessLogic.Services;

public class TaskService : ITaskService
{
    private readonly IEntityRepository<Guid, AppTask> repository;
    private readonly IMapper mapper;

    public TaskService(IEntityRepository<Guid, AppTask> repository, IMapper mapper)
    {
        this.repository = repository;
        this.mapper = mapper;
    }

    public async Task<TaskDto> Create(Guid userId, TaskCreateUpdateDto dto)
    {
        var taskEntity = mapper.Map<AppTask>(dto);
        taskEntity.Id = Guid.NewGuid();
        taskEntity.UserId = userId;
        taskEntity.CreatedAt = DateTime.UtcNow;
        taskEntity.UpdatedAt = DateTime.UtcNow;

        await repository.Create(taskEntity);

        return mapper.Map<TaskDto>(taskEntity);
    }

    public async Task<PagedResult<TaskDto>> Get(Guid userId, TasksQueryDto query)
    {
        Expression<Func<AppTask, bool>> whereExpression = task =>
            task.UserId == userId &&
            (!query.Status.HasValue || task.Status == query.Status.Value) &&
            (!query.Priority.HasValue || task.Priority == query.Priority.Value) &&
            (!query.DueFrom.HasValue || (task.DueDate.HasValue && task.DueDate.Value >= query.DueFrom.Value)) &&
            (!query.DueTo.HasValue || (task.DueDate.HasValue && task.DueDate.Value <= query.DueTo.Value));

        var order = new Dictionary<Expression<Func<AppTask, object>>, SortDirection>();
        if (!string.IsNullOrWhiteSpace(query.SortBy))
        {
            if (string.Equals(query.SortBy, nameof(AppTask.DueDate), StringComparison.OrdinalIgnoreCase))
                order.Add(x => x.DueDate ?? DateTime.MaxValue, query.Direction);
            else if (string.Equals(query.SortBy, nameof(AppTask.Priority), StringComparison.OrdinalIgnoreCase))
                order.Add(x => x.Priority, query.Direction);
        }

        var page = query.Page <= 0 ? 1 : query.Page;
        var pageSize = query.PageSize <= 0 ? 20 : query.PageSize;
        var skip = (page - 1) * pageSize;

        var totalCount = await repository.Count(whereExpression);
        var taskEntities = repository.Get(skip: skip, take: pageSize, whereExpression: whereExpression, orderBy: order, asNoTracking: true).ToList();
        var items = mapper.Map<List<TaskDto>>(taskEntities);

        return new PagedResult<TaskDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<TaskDto?> GetById(Guid userId, Guid taskId)
    {
        var taskEntities = await repository.GetByFilter(x => x.Id == taskId && x.UserId == userId);
        var taskEntity = taskEntities.FirstOrDefault();
        return taskEntity == null ? null : mapper.Map<TaskDto>(taskEntity);
    }

    public async Task<TaskDto?> Update(Guid userId, Guid taskId, TaskCreateUpdateDto dto)
    {
        var taskEntities = await repository.GetByFilter(x => x.Id == taskId && x.UserId == userId);
        var taskEntity = taskEntities.FirstOrDefault();
        if (taskEntity == null) return null;

        mapper.Map(dto, taskEntity);
        taskEntity.UpdatedAt = DateTime.UtcNow;
        await repository.Update(taskEntity);

        return mapper.Map<TaskDto>(taskEntity);
    }

    public async Task<bool> Delete(Guid userId, Guid taskId)
    {
        var taskEntities = await repository.GetByFilter(x => x.Id == taskId && x.UserId == userId);
        var taskEntity = taskEntities.FirstOrDefault();
        if (taskEntity == null) return false;

        await repository.Delete(taskEntity);
        return true;
    }
}
