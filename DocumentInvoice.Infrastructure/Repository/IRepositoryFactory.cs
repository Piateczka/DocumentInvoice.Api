using Microsoft.EntityFrameworkCore;

namespace DocumentInvoice.Infrastructure.Repository;

public interface IRepositoryFactory<TContext> where TContext : DbContext
{
    IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
