using System.ComponentModel.DataAnnotations;

namespace Ops.Host.Common.Domain;

/// <summary>
/// 表示实现此类的对象为实体。
/// </summary>
public abstract class Entity
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// 实体是否是临时的。
    /// </summary>
    /// <returns></returns>
    public bool IsTransient()
    {
        return Id <= 0;
    }
}
