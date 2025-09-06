using Bogus;
using TaskManager.DataAccess.Enums;
using TaskManager.DataAccess.Models;

namespace TaskManager.Tests.TestDataGenerators;

public static class AppTaskGenerator
{
    private static readonly Faker<AppTask> faker = new Faker<AppTask>()
        .RuleFor(x => x.Id, _ => Guid.NewGuid())
        .RuleFor(x => x.Title, f => f.Lorem.Sentence(3))
        .RuleFor(x => x.Description, f => f.Lorem.Sentence(10))
        .RuleFor(x => x.DueDate, f => f.Date.Future(1))
        .RuleFor(x => x.Status, f => f.PickRandom<Status>())
        .RuleFor(x => x.Priority, f => f.PickRandom<Priority>())
        .RuleFor(x => x.CreatedAt, f => f.Date.Past(1))
        .RuleFor(x => x.UpdatedAt, (f, t) => t.CreatedAt.AddDays(f.Random.Int(0, 30)));

    /// <summary>
    /// Generates a single <see cref="AppTask"/> with random data.
    /// </summary>
    public static AppTask Generate() => faker.Generate();

    /// <summary>
    /// Generates a list of <see cref="AppTask"/> objects.
    /// </summary>
    public static List<AppTask> Generate(int count) => faker.Generate(count);

    public static AppTask WithUser(this AppTask task, User user)
    {
        _ = task ?? throw new ArgumentNullException(nameof(task));

        task.User = user;
        task.UserId = user.Id;

        return task;
    }

    public static List<AppTask> WithUser(this List<AppTask> tasks, User user)
    {
        _ = tasks ?? throw new ArgumentNullException(nameof(tasks));

        tasks.ForEach(x => x.WithUser(user));

        return tasks;
    }
}