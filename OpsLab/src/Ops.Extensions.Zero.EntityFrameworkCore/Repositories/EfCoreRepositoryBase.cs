using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Ops.Extensions.Zero.Domain.Entities;
using Ops.Extensions.Zero.Domain.Repositories;

namespace Ops.Extensions.Zero.EntityFrameworkCore.Repositories;

public class EfCoreRepositoryBase<TDbContext, TEntity, TPrimaryKey> : RepositoryBase<TEntity, TPrimaryKey>
    where TEntity : class, IEntity<TPrimaryKey>
    where TDbContext : DbContext
{
    private readonly IDbContextProvider<TDbContext> _dbContextProvider;


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="dbContextProvider"></param>
    public EfCoreRepositoryBase(IDbContextProvider<TDbContext> dbContextProvider)
    {
        _dbContextProvider = dbContextProvider;
    }


    /// <summary>
    /// Gets EF DbContext object.
    /// </summary>
    public virtual TDbContext GetContext()
    {
        return _dbContextProvider.GetDbContext();
    }

    /// <summary>
    /// Gets EF DbContext object.
    /// </summary>
    public virtual async Task<TDbContext> GetContextAsync()
    {
        return await _dbContextProvider.GetDbContextAsync();
    }

    /// <summary>
    /// Gets DbSet for given entity.
    /// </summary>
    public virtual async Task<DbSet<TEntity>> GetTableAsync()
    {
        var context = await GetContextAsync();
        return context.Set<TEntity>();
    }

    /// <summary>
    /// Gets DbSet for given entity.
    /// </summary>
    public virtual DbSet<TEntity> GetTable()
    {
        return GetContext().Set<TEntity>();
    }

    protected virtual IQueryable<TEntity> GetQueryable()
    {
        return GetTable().AsQueryable();
    }

    protected virtual async Task<IQueryable<TEntity>> GetQueryableAsync()
    {
        return (await GetTableAsync()).AsQueryable();
    }

    public virtual DbConnection GetConnection()
    {
        var connection = GetContext().Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        return connection;
    }

    public virtual async Task<DbConnection> GetConnectionAsync()
    {
        var context = await GetContextAsync();
        var connection = context.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync();
        }

        return connection;
    }

    public override IQueryable<TEntity> GetAll()
    {
        return GetAllIncluding();
    }

    public override async Task<IQueryable<TEntity>> GetAllAsync()
    {
        return await GetAllIncludingAsync();
    }

    public override IQueryable<TEntity> GetAllIncluding(
        params Expression<Func<TEntity, object>>[] propertySelectors)
    {
        var query = GetQueryable();

        if (propertySelectors?.Any() == false)
        {
            return query;
        }

        foreach (var propertySelector in propertySelectors)
        {
            query = query.Include(propertySelector);
        }

        return query;
    }

    public override async Task<IQueryable<TEntity>> GetAllIncludingAsync(
        params Expression<Func<TEntity, object>>[] propertySelectors)
    {
        var query = await GetQueryableAsync();

        if (propertySelectors?.Any() == false)
        {
            return query;
        }

        foreach (var propertySelector in propertySelectors)
        {
            query = query.Include(propertySelector);
        }

        return query;
    }


    public override async Task<List<TEntity>> GetAllListAsync()
    {
        return await (await GetAllAsync()).ToListAsync();
    }

    public override async Task<List<TEntity>> GetAllListAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await (await GetAllAsync()).Where(predicate).ToListAsync();
    }

    public override async Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await (await GetAllAsync()).SingleAsync(predicate);
    }

    public override async Task<TEntity?> FirstOrDefaultAsync(TPrimaryKey id)
    {
        return await (await GetAllAsync()).FirstOrDefaultAsync(CreateEqualityExpressionForId(id));
    }

    public override async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await (await GetAllAsync()).FirstOrDefaultAsync(predicate);
    }

    public override TEntity Insert(TEntity entity)
    {
        return GetTable().Add(entity).Entity;
    }

    public override Task<TEntity> InsertAsync(TEntity entity)
    {
        return Task.FromResult(Insert(entity));
    }

    public override TPrimaryKey InsertAndGetId(TEntity entity)
    {
        entity = Insert(entity);

        if (MayHaveTemporaryKey(entity) || entity.IsTransient())
        {
            GetContext().SaveChanges();
        }

        return entity.Id;
    }

    public override async Task<TPrimaryKey> InsertAndGetIdAsync(TEntity entity)
    {
        entity = await InsertAsync(entity);

        if (MayHaveTemporaryKey(entity) || entity.IsTransient())
        {
            var context = await GetContextAsync();
            await context.SaveChangesAsync();
        }

        return entity.Id;
    }

    public override TPrimaryKey InsertOrUpdateAndGetId(TEntity entity)
    {
        entity = InsertOrUpdate(entity);

        if (MayHaveTemporaryKey(entity) || entity.IsTransient())
        {
            GetContext().SaveChanges();
        }

        return entity.Id;
    }

    public override async Task<TPrimaryKey> InsertOrUpdateAndGetIdAsync(TEntity entity)
    {
        entity = await InsertOrUpdateAsync(entity);

        if (MayHaveTemporaryKey(entity) || entity.IsTransient())
        {
            var context = await GetContextAsync();
            await context.SaveChangesAsync();
        }

        return entity.Id;
    }

    public override TEntity Update(TEntity entity)
    {
        AttachIfNot(entity);
        GetContext().Entry(entity).State = EntityState.Modified;
        return entity;
    }

    public override Task<TEntity> UpdateAsync(TEntity entity)
    {
        entity = Update(entity);
        return Task.FromResult(entity);
    }

    public override void Delete(TEntity entity)
    {
        AttachIfNot(entity);
        GetTable().Remove(entity);
    }

    public override void Delete(TPrimaryKey id)
    {
        var entity = GetFromChangeTrackerOrNull(id);
        if (entity != null)
        {
            Delete(entity);
            return;
        }

        entity = FirstOrDefault(id);
        if (entity != null)
        {
            Delete(entity);
            return;
        }

        //Could not found the entity, do nothing.
    }

    public override async Task<int> CountAsync()
    {
        return await (await GetAllAsync()).CountAsync();
    }

    public override async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await (await GetAllAsync()).Where(predicate).CountAsync();
    }

    public override async Task<long> LongCountAsync()
    {
        return await (await GetAllAsync()).LongCountAsync();
    }

    public override async Task<long> LongCountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await (await GetAllAsync()).Where(predicate).LongCountAsync();
    }

    protected virtual void AttachIfNot(TEntity entity)
    {
        var entry = GetContext().ChangeTracker.Entries().FirstOrDefault(ent => ent.Entity == entity);
        if (entry != null)
        {
            return;
        }

        GetTable().Attach(entity);
    }

    public DbContext GetDbContext()
    {
        return GetContext();
    }

    public async Task<DbContext> GetDbContextAsync()
    {
        return await GetContextAsync();
    }

    public async Task EnsureCollectionLoadedAsync<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, IEnumerable<TProperty>>> collectionExpression,
        CancellationToken cancellationToken)
        where TProperty : class
    {
        var context = await GetContextAsync();
        await context.Entry(entity).Collection(collectionExpression).LoadAsync(cancellationToken);
    }

    public void EnsureCollectionLoaded<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, IEnumerable<TProperty>>> collectionExpression,
        CancellationToken cancellationToken)
        where TProperty : class
    {
        GetContext().Entry(entity).Collection(collectionExpression).Load();
    }

    public async Task EnsurePropertyLoadedAsync<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, TProperty>> propertyExpression,
        CancellationToken cancellationToken)
        where TProperty : class
    {
        var context = await GetContextAsync();
        await context.Entry(entity).Reference(propertyExpression).LoadAsync(cancellationToken);
    }

    public void EnsurePropertyLoaded<TProperty>(
        TEntity entity,
        Expression<Func<TEntity, TProperty>> propertyExpression,
        CancellationToken cancellationToken)
        where TProperty : class
    {
        GetContext().Entry(entity).Reference(propertyExpression).Load();
    }

    private TEntity? GetFromChangeTrackerOrNull(TPrimaryKey id)
    {
        var entry = GetContext().ChangeTracker.Entries()
            .FirstOrDefault(
                ent =>
                    ent.Entity is TEntity &&
                    EqualityComparer<TPrimaryKey>.Default.Equals(id, (ent.Entity as TEntity).Id)
            );

        return entry?.Entity as TEntity;
    }

    private static bool MayHaveTemporaryKey(TEntity entity)
    {
        if (typeof(TPrimaryKey) == typeof(byte))
        {
            return true;
        }

        if (typeof(TPrimaryKey) == typeof(int))
        {
            return Convert.ToInt32(entity.Id) <= 0;
        }

        if (typeof(TPrimaryKey) == typeof(long))
        {
            return Convert.ToInt64(entity.Id) <= 0;
        }

        return false;
    }
}
