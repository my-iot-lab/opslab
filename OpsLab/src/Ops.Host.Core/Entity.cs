using FreeSql.DataAnnotations;

namespace Ops.Host.Core;

/// <summary>
/// 表示实现对象为实体。
/// </summary>
public abstract class Entity
{
    [Column(IsPrimary = true, IsIdentity = true)]
    public int Id { get; set; }

    public bool IsTransient()
    {
        return Id <= 0;
    }
}
