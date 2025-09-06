using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManager.Tests.Database;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TaskManager.DataAccess;
using TaskManager.DataAccess.Enums;
using TaskManager.DataAccess.Models;
using TaskManager.DataAccess.Repository.Base;

[TestFixture]
public class EntityRepositoryTests
{
    private TestTaskManagerDbContext dbContext;
    private EntityRepository<Guid, TestEntity> repository;

    [SetUp]
    public void SetUp()
    {
        var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=TaskManagerTestDb;Trusted_Connection=True;MultipleActiveResultSets=True")
            .Options;

        dbContext = new TestTaskManagerDbContext(options);
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
        repository = new EntityRepository<Guid, TestEntity>(dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        dbContext?.Dispose();
    }

    [Test]
    public async Task Create_Single_AddsAndReturnsEntity()
    {
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        var created = await repository.Create(entity);
        Assert.That(created, Is.SameAs(entity));
        var fromDb = await repository.GetById(entity.Id);
        Assert.That(fromDb, Is.Not.Null);
        Assert.That(fromDb!.Name, Is.EqualTo("A"));
    }

    [Test]
    public async Task Create_Multiple_AddsAndReturnsEntities()
    {
        var list = new[]
        {
            new TestEntity { Id = Guid.NewGuid(), Name = "A" },
            new TestEntity { Id = Guid.NewGuid(), Name = "B" }
        };
        var created = await repository.Create(list);
        Assert.That(created, Is.EquivalentTo(list));
        var all = await repository.GetAll();
        Assert.That(all.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task Update_ModifiesEntity()
    {
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        await repository.Create(entity);
        entity.Name = "B";
        var updated = await repository.Update(entity);
        Assert.That(updated.Name, Is.EqualTo("B"));
        var fromDb = await repository.GetById(entity.Id);
        Assert.That(fromDb!.Name, Is.EqualTo("B"));
    }

    [Test]
    public async Task Delete_RemovesEntity()
    {
        var e = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        await repository.Create(e);
        await repository.Delete(e);
        var fromDb = await repository.GetById(e.Id);
        Assert.That(fromDb, Is.Null);
    }

    [Test]
    public async Task GetById_ReturnsCorrectEntity()
    {
        var entity1 = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        var entity2 = new TestEntity { Id = Guid.NewGuid(), Name = "B" };
        await repository.Create(new[] { entity1, entity2 });
        var fromDb = await repository.GetById(entity2.Id);
        Assert.That(fromDb!.Name, Is.EqualTo("B"));
    }

    [Test]
    public async Task GetAll_ReturnsAll()
    {
        await repository.Create(new[]
        {
            new TestEntity { Id = Guid.NewGuid(), Name = "A" },
            new TestEntity { Id = Guid.NewGuid(), Name = "B" },
            new TestEntity { Id = Guid.NewGuid(), Name = "C" }
        });
        var all = await repository.GetAll();
        Assert.That(all.Count(), Is.EqualTo(3));
    }

    [Test]
    public async Task GetByFilter_ReturnsFiltered()
    {
        await repository.Create(new[]
        {
            new TestEntity { Id = Guid.NewGuid(), Name = "A" },
            new TestEntity { Id = Guid.NewGuid(), Name = "B" },
            new TestEntity { Id = Guid.NewGuid(), Name = "A" }
        });
        var filtered = await repository.GetByFilter(x => x.Name == "A");
        Assert.That(filtered.Count(), Is.EqualTo(2));
        Assert.That(filtered.All(x => x.Name == "A"), Is.True);
    }

    [Test]
    public async Task GetByFilterNoTracking_ReturnsFilteredAndNoTracking()
    {
        await repository.Create(new[]
        {
            new TestEntity { Id = Guid.NewGuid(), Name = "A" },
            new TestEntity { Id = Guid.NewGuid(), Name = "B" }
        });
        var query = repository.GetByFilterNoTracking(x => x.Name == "B");
        var list = query.ToList();
        Assert.That(list.Count, Is.EqualTo(1));
        var entry = dbContext.Entry(list[0]);
        Assert.That(entry.State == EntityState.Detached || entry.State == EntityState.Unchanged, Is.True);
    }

    [Test]
    public async Task Get_SkipTakeAndOrder_Works()
    {
        await repository.Create(new[]
        {
            new TestEntity { Id = Guid.NewGuid(), Name = "C" },
            new TestEntity { Id = Guid.NewGuid(), Name = "A" },
            new TestEntity { Id = Guid.NewGuid(), Name = "B" },
            new TestEntity { Id = Guid.NewGuid(), Name = "D" }
        });
        var order = new Dictionary<Expression<Func<TestEntity, object>>, SortDirection>
        {
            { x => x.Name, SortDirection.Ascending }
        };
        var page = repository.Get(skip: 1, take: 2, orderBy: order).ToList();
        Assert.That(page.Select(x => x.Name), Is.EqualTo(new[] { "B", "C" }));
    }

    [Test]
    public async Task Any_WorksWithAndWithoutPredicate()
    {
        Assert.That(await repository.Any(), Is.False);
        await repository.Create(new TestEntity { Id = Guid.NewGuid(), Name = "A" });
        Assert.That(await repository.Any(), Is.True);
        Assert.That(await repository.Any(x => x.Name == "A"), Is.True);
        Assert.That(await repository.Any(x => x.Name == "B"), Is.False);
    }

    [Test]
    public async Task Count_WorksWithAndWithoutPredicate()
    {
        await repository.Create(new[]
        {
            new TestEntity { Id = Guid.NewGuid(), Name = "A" },
            new TestEntity { Id = Guid.NewGuid(), Name = "B" },
            new TestEntity { Id = Guid.NewGuid(), Name = "A" }
        });
        Assert.That(await repository.Count(), Is.EqualTo(3));
        Assert.That(await repository.Count(x => x.Name == "A"), Is.EqualTo(2));
    }

    [Test]
    public async Task SaveChangesAsync_WrapsDbContext()
    {
        dbContext.Add(new TestEntity { Id = Guid.NewGuid(), Name = "A" });
        var affected = await repository.SaveChangesAsync();
        Assert.That(affected, Is.EqualTo(1));
    }

    [Test]
    public void SaveChanges_WrapsDbContext()
    {
        dbContext.Add(new TestEntity { Id = Guid.NewGuid(), Name = "A" });
        var affected = repository.SaveChanges();
        Assert.That(affected, Is.EqualTo(1));
    }

    [Test]
    public async Task RunInTransaction_ReturnsResultAndCommits()
    {
        var id = Guid.NewGuid();
        var result = await repository.RunInTransaction(async () =>
        {
            await repository.Create(new TestEntity { Id = id, Name = "A" });
            return 42;
        });
        Assert.That(result, Is.EqualTo(42));
        var fromDb = await repository.GetById(id);
        Assert.That(fromDb, Is.Not.Null);
    }

    [Test]
    public async Task RunInTransaction_Void_Commits()
    {
        var id = Guid.NewGuid();
        await repository.RunInTransaction(async () =>
        {
            await repository.Create(new TestEntity { Id = id, Name = "A" });
        });
        var fromDb = await repository.GetById(id);
        Assert.That(fromDb, Is.Not.Null);
    }

    [Test]
    public async Task RunInTransaction_RollsBackOnException()
    {
        var id = Guid.NewGuid();
        try
        {
            await repository.RunInTransaction(async () =>
            {
                await repository.Create(new TestEntity { Id = id, Name = "A" });
                throw new InvalidOperationException();
            });
        }
        catch
        {
        }
        var fromDb = await repository.GetById(id);
        Assert.That(fromDb, Is.Null);
    }
}

public class TestTaskManagerDbContext : TaskManagerDbContext
{
    public TestTaskManagerDbContext(DbContextOptions<TaskManagerDbContext> options) : base(options) { }

    public DbSet<TestEntity> TestEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<TestEntity>().HasKey(x => x.Id);
    }
}

public class TestEntity : IKeyedEntity<Guid>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

