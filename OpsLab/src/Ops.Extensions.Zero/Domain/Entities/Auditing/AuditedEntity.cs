namespace Ops.Extensions.Zero.Domain.Entities.Auditing;

/// <summary>
/// 带有审计信息的实体
/// </summary>
public abstract class AuditedEntity : AuditedEntity<int>, IEntity
{

}

/// <summary>
/// 带有审计信息的实体
/// </summary>
/// <typeparam name="TPrimaryKey"></typeparam>
public abstract class AuditedEntity<TPrimaryKey> : Entity<TPrimaryKey>, IShouldHaveCreateTime, IShouldHaveUpdateTime
{
    public TPrimaryKey? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public TPrimaryKey? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 表示实现实体有创建时间
/// </summary>
public interface IShouldHaveCreateTime
{
    DateTime CreatedAt { get; set; }
}

/// <summary>
/// 表示实现实体有更新时间
/// </summary>
public interface IShouldHaveUpdateTime
{
    DateTime? UpdatedAt { get; set; }
}