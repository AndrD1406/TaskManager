using System.Linq.Expressions;
using TaskManager.DataAccess.Enums;
using TaskManager.DataAccess.Models;

namespace TaskManager.DataAccess.Repository.Base;

public interface IEntityRepository<TKey, TEntity>
where TEntity : class, IKeyedEntity<TKey>, new()
where TKey : IEquatable<TKey>
{
    public Task<TEntity> Create(TEntity entity);

    public Task<IEnumerable<TEntity>> Create(IEnumerable<TEntity> entities);

    public Task<T> RunInTransaction<T>(Func<Task<T>> operation);

    public Task RunInTransaction(Func<Task> operation);

    public Task Delete(TEntity entity);

    public Task<IEnumerable<TEntity>> GetAll();

    public Task<IEnumerable<TEntity>> GetAllWithDetails(string includeProperties = "");

    public Task<IEnumerable<TEntity>> GetByFilter(Expression<Func<TEntity, bool>> whereExpression, string includeProperties = "");

    public IQueryable<TEntity> GetByFilterNoTracking(Expression<Func<TEntity, bool>> whereExpression, string includeProperties = "");

    public Task<TEntity> GetById(TKey id);

    public Task<TEntity> GetByIdWithDetails(TKey id, string includeProperties = "");

    public Task<TEntity> Update(TEntity entity);

    public Task<TEntity> ReadAndUpdateWith<TDto>(TDto dto, Func<TDto, TEntity, TEntity> map)
        where TDto : IDto<TEntity, TKey>;

    public IQueryable<TEntity> Get(
        int skip = 0,
        int take = 0,
        string includeProperties = "",
        Expression<Func<TEntity, bool>> whereExpression = null,
        Dictionary<Expression<Func<TEntity, object>>, SortDirection> orderBy = null,
        bool asNoTracking = false);

    public Task<bool> Any(Expression<Func<TEntity, bool>> whereExpression = null);

    public Task<int> Count(Expression<Func<TEntity, bool>> whereExpression = null);

    public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess = true, CancellationToken cancellationToken = default);

    public int SaveChanges(bool acceptAllChangesOnSuccess = true);
}
