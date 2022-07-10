using Ops.Extensions.Zero.Domain.Uow;

namespace Ops.Extensions.Zero.EntityFrameworkCore.Uow;

/// <summary>
/// 工作单元
/// </summary>
/// <typeparam name="TDbContext"></typeparam>
public class EfCoreUnitOfWork<TDbContext> : IUnitOfWork where TDbContext : OpsDbContext
{
    private readonly TDbContext _dbContext;

    public EfCoreUnitOfWork(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public int SaveChanges()
    {
        return _dbContext.SaveChanges();
    }

    public Task<int> SaveChangesAsync()
    {
        return _dbContext.SaveChangesAsync();
    }
}
