using Microsoft.EntityFrameworkCore;

namespace Ops.Extensions.Zero.EntityFrameworkCore;

public sealed class SimpleDbContextProvider<TDbContext> : IDbContextProvider<TDbContext>
    where TDbContext : DbContext
{
    public TDbContext DbContext { get; }

    public SimpleDbContextProvider(TDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public TDbContext GetDbContext()
    {
        return DbContext;
    }

    public Task<TDbContext> GetDbContextAsync()
    {
        return Task.FromResult(DbContext);
    }
}
