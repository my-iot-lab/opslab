using Microsoft.EntityFrameworkCore;

namespace Ops.Extensions.Zero.EntityFrameworkCore;

public interface IDbContextProvider<TDbContext>
        where TDbContext : DbContext
{
    TDbContext GetDbContext();

    Task<TDbContext> GetDbContextAsync();
}
