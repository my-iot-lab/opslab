namespace Ops.Extensions.Zero.Domain.Uow;

/// <summary>
/// 表示继承类为工作单元
/// </summary>
public interface IUnitOfWork
{
    int SaveChanges();

    Task<int> SaveChangesAsync();
}
