namespace Ops.Extensions.Zero.Domain.Entities;

public interface ISoftDelete
{
    /// <summary>
    /// 标记是否实体已经被删除。
    /// </summary>
    bool IsDeleted { get; set; }
}
