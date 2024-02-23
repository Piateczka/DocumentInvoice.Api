using DocumentInvoice.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace DocumentInvoice.Infrastructure.Repository
{
    public class Repository<TEntity, TContext> : IRepository<TEntity>
        where TEntity : class
        where TContext : DbContext
    {
        public readonly TContext _dbContext;

        public IQueryable<TEntity> Query => _dbContext.Set<TEntity>();

        public Repository(TContext dbContext)
        {
            _dbContext = dbContext;
        }

        public ValueTask<TEntity> FindAsync(int id, CancellationToken cancellationToken)
        {
            return _dbContext.Set<TEntity>().FindAsync(new object[] { id }, cancellationToken);
        }

        public void Update(TEntity entity)
        {
            _dbContext.Entry(entity).State = EntityState.Modified;
        }

        public void Update(IEnumerable<TEntity> entities)
        {
            foreach (var entity in entities)
            {
                _dbContext.Entry(entity).State = EntityState.Modified;
            }
        }

        public ValueTask<EntityEntry<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken)
        {
            return _dbContext.Set<TEntity>().AddAsync(entity, cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken)
        {
            await _dbContext.Set<TEntity>().AddRangeAsync(entities, cancellationToken);
        }

        public void Delete(TEntity entity)
        {
            _dbContext.Set<TEntity>().Remove(entity);
        }

        public void DeleteRange(IEnumerable<TEntity> entities)
        {
            _dbContext.Set<TEntity>().RemoveRange(entities);
        }

        public async Task<IEnumerable<TEntity>> ListAsync(CancellationToken cancellationToken)
        {
            return await _dbContext.Set<TEntity>().ToListAsync(cancellationToken);
        }
    }
}
