using AutoMapper;
using Moq;
using System.Linq.Expressions;
using TaskManager.BusinessLogic.Dtos.Task;
using TaskManager.BusinessLogic.Services;
using TaskManager.BusinessLogic.Util;
using TaskManager.DataAccess.Enums;
using TaskManager.DataAccess.Models;
using TaskManager.DataAccess.Repository.Base;

namespace TaskManager.Tests.Services;

[TestFixture]
public class TaskServiceTests
{
    private IMapper mapper;

    [SetUp]
    public void SetUp()
    {
        var config = new MapperConfiguration(c => c.AddProfile<MappingProfile>());
        mapper = config.CreateMapper();
    }

    [Test]
    public async Task Create_SetsUserId_AndReturnsDto()
    {
        // Arrange
        var repository = new Mock<IEntityRepository<Guid, AppTask>>();
        repository.Setup(r => r.Create(It.IsAny<AppTask>())).ReturnsAsync((AppTask t) => t);
        var service = new TaskService(repository.Object, mapper);
        var dto = new TaskCreateUpdateDto { Title = "T", Description = "D", DueDate = DateTime.UtcNow, Status = Status.Pending, Priority = Priority.Medium };
        var userId = Guid.NewGuid();

        // Act
        var result = await service.Create(userId, dto);

        // Assert
        Assert.That(result.Title, Is.EqualTo("T"));
        repository.Verify(r => r.Create(It.Is<AppTask>(t => t.UserId == userId && t.Title == "T")), Times.Once);
    }

    [Test]
    public async Task Get_ReturnsPagedFilteredSorted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var data = new List<AppTask>
        {
            new AppTask { Id = Guid.NewGuid(), UserId = userId, Title = "A", Priority = Priority.High, Status = Status.Pending, DueDate = DateTime.UtcNow.AddDays(1), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new AppTask { Id = Guid.NewGuid(), UserId = userId, Title = "B", Priority = Priority.Low, Status = Status.Completed, DueDate = DateTime.UtcNow.AddDays(2), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };
        var repository = new Mock<IEntityRepository<Guid, AppTask>>();
        repository.Setup(r => r.Count(It.IsAny<Expression<Func<AppTask, bool>>>())).ReturnsAsync(2);
        repository.Setup(r => r.Get(
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<string>(),
            It.IsAny<Expression<Func<AppTask, bool>>>(),
            It.IsAny<Dictionary<Expression<Func<AppTask, object>>, SortDirection>>(),
            It.IsAny<bool>()))
           .Returns((int skip, int take, string _, Expression<Func<AppTask, bool>> where, Dictionary<Expression<Func<AppTask, object>>, SortDirection> order, bool _) =>
           {
               var query = data.AsQueryable().Where(where);

               if (order != null && order.Any())
               {
                   var first = order.First();
                   var selector = first.Key.Compile();
                   query = first.Value == SortDirection.Ascending
                       ? query.OrderBy(selector).AsQueryable()
                       : query.OrderByDescending(selector).AsQueryable();
               }

               if (skip > 0) query = query.Skip(skip);
               if (take > 0) query = query.Take(take);

               return query;
           });
        var service = new TaskService(repository.Object, mapper);
        var queryDto = new TasksQueryDto { Page = 1, PageSize = 10, SortBy = nameof(AppTask.DueDate) };

        // Act
        var result = await service.Get(userId, queryDto);

        // Assert
        Assert.That(result.TotalCount, Is.EqualTo(2));
        Assert.That(result.Items.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task GetById_ReturnsNull_WhenDifferentOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var repository = new Mock<IEntityRepository<Guid, AppTask>>();
        repository.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<AppTask, bool>>>(), It.IsAny<string>()))
        .ReturnsAsync((Expression<Func<AppTask, bool>> predicate, string _) =>
        {
            var data = new List<AppTask> { new AppTask { Id = taskId, UserId = otherUserId, Title = "X" } };
            return data.AsQueryable().Where(predicate).ToList();
        });
        var service = new TaskService(repository.Object, mapper);

        // Act
        var result = await service.GetById(userId, taskId);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task Update_Updates_WhenOwned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var entity = new AppTask { Id = taskId, UserId = userId, Title = "Old", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var repository = new Mock<IEntityRepository<Guid, AppTask>>();
        repository.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<AppTask, bool>>>(), It.IsAny<string>())).ReturnsAsync(new List<AppTask> { entity });
        repository.Setup(r => r.Update(It.IsAny<AppTask>())).ReturnsAsync((AppTask t) => t);
        var service = new TaskService(repository.Object, mapper);

        // Act
        var result = await service.Update(userId, taskId, new TaskCreateUpdateDto { Title = "New", Status = Status.InProgress, Priority = Priority.High });

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result!.Title, Is.EqualTo("New"));
        repository.Verify(r => r.Update(It.Is<AppTask>(t => t.Id == taskId && t.Title == "New")), Times.Once);
    }

    [Test]
    public async Task Delete_ReturnsFalse_WhenNotOwned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var repository = new Mock<IEntityRepository<Guid, AppTask>>();
        repository.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<AppTask, bool>>>(), It.IsAny<string>())).ReturnsAsync(Enumerable.Empty<AppTask>());
        var service = new TaskService(repository.Object, mapper);

        // Act
        var ok = await service.Delete(userId, taskId);

        // Assert
        Assert.That(ok, Is.False);
    }

    [Test]
    public async Task Delete_Deletes_WhenOwned()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var entity = new AppTask { Id = taskId, UserId = userId, Title = "T" };
        var repository = new Mock<IEntityRepository<Guid, AppTask>>();
        repository.Setup(r => r.GetByFilter(It.IsAny<Expression<Func<AppTask, bool>>>(), It.IsAny<string>())).ReturnsAsync(new List<AppTask> { entity });
        repository.Setup(r => r.Delete(It.IsAny<AppTask>())).Returns(Task.CompletedTask);
        var service = new TaskService(repository.Object, mapper);

        // Act
        var ok = await service.Delete(userId, taskId);

        // Assert
        Assert.That(ok, Is.True);
        repository.Verify(r => r.Delete(It.Is<AppTask>(t => t.Id == taskId)), Times.Once);
    }
}

