using Ops.Extensions.Zero.EntityFrameworkCore.Repositories;
using Ops.Extensions.Zero.EntityFrameworkCore.Tests.Domain;

namespace Ops.Extensions.Zero.EntityFrameworkCore.Tests.Ef;

public class ProductRepository : EfCoreRepositoryBase<ProductDbContext, Product>, IProductRepository
{
    public ProductRepository(IDbContextProvider<ProductDbContext> dbContextProvider)
        : base(dbContextProvider)
    {

    }
}
