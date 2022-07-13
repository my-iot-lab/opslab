using Microsoft.EntityFrameworkCore;
using Ops.Extensions.Zero.EntityFrameworkCore.Tests.Ef;
using Ops.Extensions.Zero.EntityFrameworkCore.Uow;

namespace Ops.Extensions.Zero.EntityFrameworkCore.Tests;

public class Repository_MySql_Tests
{
    private const string ConnectionString = "server=localhost;database=mes;user=root;password=noke@1234;";

    [Fact]
    public void Should_Get_Product_Test()
    {
        // 添加包 Microsoft.EntityFrameworkCore.SqlServer
        var contextOptions = new DbContextOptionsBuilder<ProductDbContext>()
            .UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString))
            .Options;

        using var context = new ProductDbContext(contextOptions);

        var dbProvider = new SimpleDbContextProvider<ProductDbContext>(context);
        var repo = new ProductRepository(dbProvider);
        var product = repo.GetAllIncluding(s => s.ProductDetails) // Including 后子集合不会为 null
                          .FirstOrDefault(s => s.Id == 1);
        Assert.NotNull(product);
        Assert.NotNull(product!.ProductDetails);
        Assert.True(product!.ProductDetails.Count > 0);
    }

    [Fact]
    public async Task Should_Insert_Product_Test()
    {
        var contextOptions = new DbContextOptionsBuilder<ProductDbContext>()
           .UseMySql(ConnectionString, ServerVersion.AutoDetect(ConnectionString))
           .Options;

        using var context = new ProductDbContext(contextOptions);
        var unitOfWork = new EfCoreUnitOfWork<ProductDbContext>(context);
        var dbProvider = new SimpleDbContextProvider<ProductDbContext>(context);
        var repo = new ProductRepository(dbProvider);
        var product = await repo.InsertAsync(new Domain.Product
        {
            Name = "P001",
            ProductDetails = new List<Domain.ProductDetail>()
            {
                new Domain.ProductDetail { Model = "M001" },
            },
        });

        unitOfWork.SaveChanges();

        Assert.NotNull(product);
    }
}
