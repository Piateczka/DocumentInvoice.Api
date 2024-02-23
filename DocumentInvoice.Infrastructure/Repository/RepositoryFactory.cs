using DocumentInvoice.Domain;
using Microsoft.EntityFrameworkCore;

namespace DocumentInvoice.Infrastructure.Repository
{
    public class RepositoryFactory<TContext> : IRepositoryFactory<TContext>
        where TContext : DbContext
    {
        private readonly TContext _dbContext;
        private readonly Dictionary<Type, object> _repositories;

        public RepositoryFactory(TContext dbContext)
        {
            _dbContext = dbContext;
            _repositories = new Dictionary<Type, object>();
        }

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            var type = typeof(TEntity);
            if (!_repositories.ContainsKey(type))
            {
                _repositories[type] = new Repository<TEntity, TContext>(_dbContext);
            }

            return (IRepository<TEntity>)_repositories[type];
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
