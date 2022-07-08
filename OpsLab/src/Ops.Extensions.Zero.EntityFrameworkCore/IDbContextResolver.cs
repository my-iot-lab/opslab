using Microsoft.EntityFrameworkCore;

namespace Ops.Extensions.Zero.EntityFrameworkCore;

public interface IDbContextResolver
{
    TDbContext Resolve<TDbContext>(string connectionString)
            where TDbContext : DbContext;
}
