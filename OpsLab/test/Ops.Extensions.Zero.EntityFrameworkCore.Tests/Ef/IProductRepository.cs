using Ops.Extensions.Zero.Domain.Repositories;
using Ops.Extensions.Zero.EntityFrameworkCore.Tests.Domain;

namespace Ops.Extensions.Zero.EntityFrameworkCore.Tests.Ef
{
    public interface IProductRepository : IRepository<Product>
    {
    }
}
