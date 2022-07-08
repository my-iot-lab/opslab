namespace Ops.Extensions.Zero.Domain.Entities;

/// <summary>
/// 表示对象为实体
/// </summary>
/// <typeparam name="TPrimaryKey">主键类型</typeparam>
public interface IEntity<TPrimaryKey>
{
    /// <summary>
    /// Unique identifier for this entity.
    /// </summary>
    TPrimaryKey Id { get; set; }

    /// <summary>
    /// Checks if this entity is transient (not persisted to database and it has not an <see cref="Id"/>).
    /// </summary>
    /// <returns>True, if this entity is transient</returns>
    bool IsTransient();
}

/// <summary>
/// 实体基类，主键为 <see cref="int"/> 类型
/// </summary>
public interface IEntity : IEntity<int>
{

}