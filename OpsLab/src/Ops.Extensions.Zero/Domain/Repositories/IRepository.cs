using System.Linq.Expressions;
using Ops.Extensions.Zero.Domain.Entities;

namespace Ops.Extensions.Zero.Domain.Repositories;

public interface IRepository
{

}

public interface IRepository<TEntity, TPrimaryKey> : IRepository where TEntity : class, IEntity<TPrimaryKey>
{
    #region Select/Get/Query

    IQueryable<TEntity> GetAll();

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

    TEntity Insert(TEntity entity);

    Task<TEntity> InsertAsync(TEntity entity);

    /// <summary>
    /// Inserts a new entity and gets it's Id.
    /// It may require to save current unit of work
    /// to be able to retrieve id.
    /// </summary>
    /// <param name="entity">Entity</param>
    /// <returns>Id of the entity</returns>
    TPrimaryKey InsertAndGetId(TEntity entity);

    /// <summary>
    /// Inserts a new entity and gets it's Id.
    /// It may require to save current unit of work
    /// to be able to retrieve id.
    /// </summary>
    /// <param name="entity">Entity</param>
    /// <returns>Id of the entity</returns>
    Task<TPrimaryKey> InsertAndGetIdAsync(TEntity entity);

    /// <summary>
    /// Inserts or updates given entity depending on Id's value.
    /// </summary>
    /// <param name="entity">Entity</param>
    TEntity InsertOrUpdate(TEntity entity);

    /// <summary>
    /// Inserts or updates given entity depending on Id's value.
    /// </summary>
    /// <param name="entity">Entity</param>
    Task<TEntity> InsertOrUpdateAsync(TEntity entity);

    /// <summary>
    /// Inserts or updates given entity depending on Id's value.
    /// Also returns Id of the entity.
    /// It may require to save current unit of work
    /// to be able to retrieve id.
    /// </summary>
    /// <param name="entity">Entity</param>
    /// <returns>Id of the entity</returns>
    TPrimaryKey InsertOrUpdateAndGetId(TEntity entity);

    /// <summary>
    /// Inserts or updates given entity depending on Id's value.
    /// Also returns Id of the entity.
    /// It may require to save current unit of work
    /// to be able to retrieve id.
    /// </summary>
    /// <param name="entity">Entity</param>
    /// <returns>Id of the entity</returns>
    Task<TPrimaryKey> InsertOrUpdateAndGetIdAsync(TEntity entity);

    #endregion

    #region Update

    TEntity Update(TEntity entity);

    Task<TEntity> UpdateAsync(TEntity entity);

    TEntity Update(TPrimaryKey id, Action<TEntity> updateAction);

    Task<TEntity> UpdateAsync(TPrimaryKey id, Func<TEntity, Task> updateAction);

    #endregion

    #region Delete

    void Delete(TEntity entity);

    Task DeleteAsync(TEntity entity);

    void Delete(TPrimaryKey id);

    Task DeleteAsync(TPrimaryKey id);

    void Delete(Expression<Func<TEntity, bool>> predicate);

    Task DeleteAsync(Expression<Func<TEntity, bool>> predicate);

    #endregion

    #region Aggregates

    int Count();

    Task<int> CountAsync();

    int Count(Expression<Func<TEntity, bool>> predicate);

    Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);

    long LongCount();

    Task<long> LongCountAsync();

    long LongCount(Expression<Func<TEntity, bool>> predicate);

    Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate);

    #endregion
}
