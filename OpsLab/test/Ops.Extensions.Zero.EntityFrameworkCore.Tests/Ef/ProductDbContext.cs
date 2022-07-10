using Microsoft.EntityFrameworkCore;
using Ops.Extensions.Zero.EntityFrameworkCore.Tests.Domain;

namespace Ops.Extensions.Zero.EntityFrameworkCore.Tests.Ef;

public sealed class ProductDbContext : OpsDbContext
{
    public ProductDbContext(DbContextOptions<ProductDbContext> options)
      : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // EF Core 6.0 默认将子实体类型标记为由其父实体拥有。 这样就无需在模型中大量调用 OwnsMany 和 OwnsOne。
        // 详细参考 https://docs.microsoft.com/zh-cn/ef/core/what-is-new/ef-core-6.0/whatsnew#cosmos-provider-enhancements
        modelBuilder.Entity<Product>().ToTable("Product");
        modelBuilder.Entity<ProductDetail>().ToTable("ProductDetail");
        modelBuilder.Entity<Person>().ToTable("Person");

        base.OnModelCreating(modelBuilder);
    }
}
