namespace Ops.Extensions.Zero.Domain.Entities;

/// <summary>
/// 实体类型
/// </summary>
/// <typeparam name="TPrimaryKey">主键类型</typeparam>
public abstract class Entity<TPrimaryKey> : IEntity<TPrimaryKey>
{
    /// <summary>
    /// 实体 Id。
    /// </summary>
    public virtual TPrimaryKey Id { get; set; }

    /// <summary>
    /// 实体是否是临时的。
    /// </summary>
    /// <returns></returns>
    public virtual bool IsTransient()
    {
        if (EqualityComparer<TPrimaryKey>.Default.Equals(Id, default))
        {
            return true;
        }

        // Workaround for EF Core since it sets int/long to min value when attaching to dbcontext
        if (typeof(TPrimaryKey) == typeof(int))
        {
            return Convert.ToInt32(Id) <= 0;
        }

        if (typeof(TPrimaryKey) == typeof(long))
        {
            return Convert.ToInt64(Id) <= 0;
        }

        return false;
    }
}

/// <summary>
/// 表示主键为 <see cref="int"/> 类型的实体
/// </summary>
public abstract class Entity : Entity<int>, IEntity
{
}
