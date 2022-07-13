using System.Linq.Expressions;
using Ops.Extensions.Zero.Domain.Entities;

namespace Ops.Extensions.Zero.Domain.Repositories;

/// <summary>
/// 表示实现对象为仓储
/// </summary>
public interface IRepository
{

}

/// <summary>
/// 表示实现对象为仓储
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
public interface IRepository<TEntity> : IRepository<TEntity, int> where TEntity : class, IEntity
{

}

/// <summary>
/// 表示实现对象为仓储
/// </summary>
/// <typeparam name="TEntity">实体类型</typeparam>
/// <typeparam name="TPrimaryKey">实体主键</typeparam>
public interface IRepository<TEntity, TPrimaryKey> : IRepository where TEntity : class, IEntity<TPrimaryKey>
{
    #region Select/Get/Query

    IQueryable<TEntity> GetAll();

    /// <summary>
    /// 对于有主从关系的对象，需要获取从表数据集合的，需要包 Include 从表数据集合。
    /// </summary>
    /// <param name="propertySelectors"></param>
    /// <returns></returns>
    IQueryable<TEntity> GetAllIncluding(params Expression<Func<TEntity, object>>[] propertySelectors);

    List<TEntity> GetAllList();

    Task<List<TEntity>> GetAllListAsync();

    List<TEntity> GetAllList(Expression<Func<TEntity, bool>> predicate);

    Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity, bool>> predicate);

    T Query<T>(Func<IQueryable<TEntity>, T> queryMethod);

    TEntity Get(TPrimaryKey id);

    Task<TEntity> GetAsync(TPrimaryKey id);

    TEntity Single(Expression<Func<TEntity, bool>> predicate);

    Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate);

    TEntity? FirstOrDefault(TPrimaryKey id);

    Task<TEntity?> FirstOrDefaultAsync(TPrimaryKey id);

    TEntity? FirstOrDefault(Expression<Func<TEntity, bool>> predicate);

    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// Creates an entity with given primary key without database access.
    /// </summary>
    /// <param name="id">Primary key of the entity to load</param>
    /// <returns>Entity</returns>
    TEntity Load(TPrimaryKey id);

    #endregion

    #region Insert

    /// <summary>
    /// 新增实体。
    /// </summary>
    /// <param name="entity">要新增的实体</param>
    /// <returns></returns>
    TEntity Insert(TEntity entity);

    /// <summary>
    /// 新增实体。
    /// </summary>
    /// <param name="entity">要新增的实体</param>
    Task<TEntity> InsertAsync(TEntity entity);

    /// <summary>
    /// 新增实体并获取其 Id。
    /// 注：这可能会导致提交当前的工作单元。
    /// </summary>
    /// <param name="entity">要新增的实体</param>
    /// <returns></returns>
    TPrimaryKey InsertAndGetId(TEntity entity);

    /// <summary>
    /// 新增实体并获取其 Id。
    /// 注：这可能会导致提交当前的工作单元。
    /// </summary>
    /// <param name="entity">要新增的实体</param>
    /// <returns></returns>
    Task<TPrimaryKey> InsertAndGetIdAsync(TEntity entity);

    /// <summary>
    /// 新增或更新实体。
    /// </summary>
    /// <param name="entity">要新增或更新的实体对象</param>
    TEntity InsertOrUpdate(TEntity entity);

    /// <summary>
    /// 新增或更新实体。
    /// </summary>
    /// <param name="entity">要新增或更新的实体对象</param>
    Task<TEntity> InsertOrUpdateAsync(TEntity entity);

    /// <summary>
    /// 新增或更新实体，并获取其 Id。
    /// 注：这可能会导致提交当前的工作单元。
    /// </summary>
    /// <param name="entity">要新增或更新的实体对象</param>
    TPrimaryKey InsertOrUpdateAndGetId(TEntity entity);

    /// <summary>
    /// 新增或更新实体，并获取其 Id。
    /// 注：这可能会导致提交当前的工作单元。
    /// </summary>
    /// <param name="entity">要新增或更新的实体对象</param>
    Task<TPrimaryKey> InsertOrUpdateAndGetIdAsync(TEntity entity);

    #endregion

    #region Update

    /// <summary>
    /// 更新实体对象。
    /// </summary>
    /// <param name="entity">要更新的实体</param>
    /// <returns></returns>
    TEntity Update(TEntity entity);

    /// <summary>
    /// 更新实体对象。
    /// </summary>
    /// <param name="entity">要更新的实体</param>
    /// <returns></returns>
    Task<TEntity> UpdateAsync(TEntity entity);

    /// <summary>
    /// 更新实体对象。
    /// </summary>
    /// <param name="id">要更新的实体 Id</param>
    /// <param name="updateAction">更新对象的委托</param>
    /// <returns></returns>
    TEntity Update(TPrimaryKey id, Action<TEntity> updateAction);

    /// <summary>
    /// 更新实体对象。
    /// </summary>
    /// <param name="id">要更新的实体 Id</param>
    /// <param name="updateAction">更新对象的委托</param>
    /// <returns></returns>
    Task<TEntity> UpdateAsync(TPrimaryKey id, Func<TEntity, Task> updateAction);

    #endregion

    #region Delete

    /// <summary>
    /// 删除实体对象。
    /// </summary>
    /// <param name="entity">要删除的实体</param>
    void Delete(TEntity entity);

    /// <summary>
    /// 删除实体对象。
    /// </summary>
    /// <param name="entity">要删除的实体</param>
    Task DeleteAsync(TEntity entity);

    /// <summary>
    /// 删除实体对象。
    /// </summary>
    /// <param name="id">要删除的实体 Id</param>
    void Delete(TPrimaryKey id);

    /// <summary>
    /// 删除实体对象。
    /// </summary>
    /// <param name="id">要删除的实体 Id</param>
    Task DeleteAsync(TPrimaryKey id);

    /// <summary>
    /// 删除实体对象。
    /// </summary>
    /// <param name="predicate">要删除的实体筛选表达式</param>
    void Delete(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// 删除实体对象。
    /// </summary>
    /// <param name="predicate">要删除的实体筛选表达式</param>
    Task DeleteAsync(Expression<Func<TEntity, bool>> predicate);

    #endregion

    #region Aggregates

    /// <summary>
    /// 获取实体数量。
    /// </summary>
    /// <returns></returns>
    int Count();

    /// <summary>
    /// 获取实体数量。
    /// </summary>
    /// <returns></returns>
    Task<int> CountAsync();

    /// <summary>
    /// 获取实体数量。
    /// </summary>
    /// <param name="predicate">筛选表达式</param>
    /// <returns></returns>
    int Count(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// 获取实体数量。
    /// </summary>
    /// <param name="predicate">筛选表达式</param>
    /// <returns></returns>
    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// 实体数量
    /// </summary>
    /// <returns></returns>
    long LongCount();

    /// <summary>
    /// 获取实体数量。
    /// </summary>
    /// <returns></returns>
    Task<long> LongCountAsync();

    /// <summary>
    /// 获取实体数量。
    /// </summary>
    /// <param name="predicate">筛选表达式</param>
    /// <returns></returns>
    long LongCount(Expression<Func<TEntity, bool>> predicate);

    /// <summary>
    /// 获取实体数量。
    /// </summary>
    /// <param name="predicate">筛选表达式</param>
    /// <returns></returns>
    Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate);

    #endregion
}
