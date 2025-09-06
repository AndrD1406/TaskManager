using Bogus;
using TaskManager.DataAccess.Models;

namespace TaskManager.Tests.TestDataGenerators;

public static class UserGenerator
{
    private static readonly Faker<User> faker = new Faker<User>()
        .RuleFor(x => x.Id, _ => Guid.NewGuid())
        .RuleFor(x => x.UserName, f => f.Internet.UserName())
        .RuleFor(x => x.Email, f => f.Internet.Email())
        .RuleFor(x => x.PasswordHash, f => BCrypt.Net.BCrypt.HashPassword("P@ssw0rd!"))
        .RuleFor(x => x.CreatedAt, f => f.Date.Past(1))
        .RuleFor(x => x.UpdatedAt, (f, u) => u.CreatedAt.AddDays(f.Random.Int(0, 30)))
        .RuleFor(x => x.Tasks, _ => new List<AppTask>());

    /// <summary>
    /// Generates a single <see cref="User"/> with random data.
    /// </summary>
    public static User Generate() => faker.Generate();

    /// <summary>
    /// Generates a list of <see cref="User"/> objects.
    /// </summary>
    public static List<User> Generate(int count) => faker.Generate(count);

    public static User WithTasks(this User user, List<AppTask> tasks)
    {
        _ = user ?? throw new ArgumentNullException(nameof(user));

        user.Tasks = tasks;
        tasks.ForEach(t =>
        {
            t.User = user;
            t.UserId = user.Id;
        });

        return user;
    }

    public static User WithGeneratedTasks(this User user, int taskCount = 3)
    {
        _ = user ?? throw new ArgumentNullException(nameof(user));

        var tasks = AppTaskGenerator.Generate(taskCount).WithUser(user);
        return user.WithTasks(tasks);
    }
}
