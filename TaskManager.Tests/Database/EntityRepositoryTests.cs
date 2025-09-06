using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Tests.Database;

using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.DataAccess;
using TaskManager.DataAccess.Enums;
using TaskManager.DataAccess.Models;
using TaskManager.DataAccess.Repository.Base;

[TestFixture]
public class EntityRepositoryTests
{
    private TaskManagerDbContext dbContext;
    private EntityRepository<Guid, User> repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()) // needs Microsoft.EntityFrameworkCore.InMemory
            .Options;

        dbContext = new TaskManagerDbContext(options);
        repository = new EntityRepository<Guid, User>(dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        dbContext.Dispose();
    }

    [Test]
    public async Task Create_AddsEntity()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), UserName = "alice", Email = "a@a.com" };

        // Act
        var created = await repository.Create(user);
        var stored = await repository.GetById(user.Id);

        // Assert
        Assert.That(created.UserName, Is.EqualTo("alice"));
        Assert.That(stored, Is.Not.Null);
    }

    [Test]
    public async Task CreateRange_AddsEntities()
    {
        // Arrange
        var users = new[]
        {
            new User { Id = Guid.NewGuid(), UserName = "u1", Email = "u1@e.com" },
            new User { Id = Guid.NewGuid(), UserName = "u2", Email = "u2@e.com" }
        };

        // Act
        await repository.Create(users);
        var all = await repository.GetAll();

        // Assert
        Assert.That(all.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task Update_ModifiesEntity()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), UserName = "old", Email = "old@e.com" };
        await repository.Create(user);
        user.UserName = "new";

        // Act
        var updated = await repository.Update(user);
        var stored = await repository.GetById(user.Id);

        // Assert
        Assert.That(updated.UserName, Is.EqualTo("new"));
        Assert.That(stored.UserName, Is.EqualTo("new"));
    }

    [Test]
    public async Task Delete_RemovesEntity()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), UserName = "bob", Email = "b@e.com" };
        await repository.Create(user);

        // Act
        await repository.Delete(user);
        var all = await repository.GetAll();

        // Assert
        Assert.That(all, Is.Empty);
    }

    [Test]
    public async Task GetByFilter_FiltersCorrectly()
    {
        // Arrange
        await repository.Create(new User { Id = Guid.NewGuid(), UserName = "tom", Email = "t@e.com" });
        await repository.Create(new User { Id = Guid.NewGuid(), UserName = "sam", Email = "s@e.com" });

        // Act
        var filtered = await repository.GetByFilter(u => u.UserName == "tom");

        // Assert
        Assert.That(filtered.Count(), Is.EqualTo(1));
        Assert.That(filtered.First().UserName, Is.EqualTo("tom"));
    }

    [Test]
    public async Task Get_ReturnsOrderedAndPaged()
    {
        // Arrange
        await repository.Create(new User { Id = Guid.NewGuid(), UserName = "b", Email = "b@e.com" });
        await repository.Create(new User { Id = Guid.NewGuid(), UserName = "a", Email = "a@e.com" });

        var order = new Dictionary<System.Linq.Expressions.Expression<Func<User, object>>, SortDirection>
        {
            { u => u.UserName, SortDirection.Ascending }
        };

        // Act
        var query = repository.Get(orderBy: order);
        var list = query.ToList();

        // Assert
        Assert.That(list.First().UserName, Is.EqualTo("a"));
    }

    [Test]
    public async Task RunInTransaction_CommitsOnSuccess()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), UserName = "inTx", Email = "tx@e.com" };

        // Act
        await repository.RunInTransaction(async () =>
        {
            await repository.Create(user);
        });

        var stored = await repository.GetById(user.Id);

        // Assert
        Assert.That(stored, Is.Not.Null);
    }

    [Test]
    public void RunInTransaction_RollsBackOnFailure()
    {
        // Arrange
        var user = new User { Id = Guid.NewGuid(), UserName = "fail", Email = "fail@e.com" };

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await repository.RunInTransaction(async () =>
            {
                await repository.Create(user);
                throw new InvalidOperationException("force rollback");
            });
        });

        var stored = repository.GetById(user.Id).Result;
        Assert.That(stored, Is.Null);
    }

    [Test]
    public async Task AnyAndCount_WorkCorrectly()
    {
        // Arrange
        await repository.Create(new User { Id = Guid.NewGuid(), UserName = "any", Email = "a@a.com" });

        // Act
        var any = await repository.Any();
        var count = await repository.Count();

        // Assert
        Assert.That(any, Is.True);
        Assert.That(count, Is.EqualTo(1));
    }
}

