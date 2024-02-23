using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DocumentInvoice.Infrastructure.Repository;

public interface IRepository<TEntity> where TEntity : class
{
    ValueTask<EntityEntry<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken);
    Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken);
    void Delete(TEntity entity);
    void DeleteRange(IEnumerable<TEntity> entities);
    ValueTask<TEntity> FindAsync(int id, CancellationToken cancellationToken);
    void Update(TEntity entity);
    void Update(IEnumerable<TEntity> entities);
    Task<IEnumerable<TEntity>> ListAsync(CancellationToken cancellationToken);
    IQueryable<TEntity> Query { get; }
}
