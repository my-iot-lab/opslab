using Microsoft.EntityFrameworkCore;

namespace Ops.Extensions.Zero.EntityFrameworkCore;

public class DefaultDbContextResolver : IDbContextResolver
{
    public TDbContext Resolve<TDbContext>(string connectionString) where TDbContext : DbContext
    {
        return default;
    }
}
