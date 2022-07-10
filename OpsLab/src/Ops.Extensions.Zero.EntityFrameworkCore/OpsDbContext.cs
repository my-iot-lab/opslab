using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Ops.Extensions.Zero.Domain.Entities;
using Ops.Extensions.Zero.Domain.Entities.Auditing;
using Ops.Extensions.Zero.Expressions;

namespace Ops.Extensions.Zero.EntityFrameworkCore;

/// <summary>
/// 基于 Ops 的 DbContext，其中可以做一些软删除等筛选设置。
/// </summary>
public abstract class OpsDbContext : DbContext
{
    private static readonly MethodInfo ConfigureGlobalFiltersMethodInfo = typeof(OpsDbContext).GetMethod(nameof(ConfigureGlobalFilters), BindingFlags.Instance | BindingFlags.NonPublic)!;

    protected virtual bool IsSoftDeleteFilterEnabled { get; set; }

    protected OpsDbContext(DbContextOptions options)
            : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            ConfigureGlobalFiltersMethodInfo
                .MakeGenericMethod(entityType.ClrType)
                .Invoke(this, new object[] { modelBuilder, entityType });
        }
    }

    /// <summary>
    /// 提交数据
    /// </summary>
    /// <returns></returns>
    /// <exception cref="DbUpdateException"/>
    /// <exception cref="DbUpdateConcurrencyException"/>
    public override int SaveChanges()
    {
        ApplyOpsConcepts();
        return base.SaveChanges();
    }

    /// <summary>
    /// 提交数据
    /// </summary>
    /// <returns></returns>
    /// <exception cref="DbUpdateException"/>
    /// <exception cref="DbUpdateConcurrencyException"/>
    /// <exception cref="OperationCanceledException"/>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyOpsConcepts();
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected virtual void ApplyOpsConcepts()
    {
        foreach (var entry in ChangeTracker.Entries().ToList())
        {
            if (entry.State != EntityState.Modified && CheckOwnedEntityChange(entry))
            {
                Entry(entry.Entity).State = EntityState.Modified;
            }

            ApplyOpsConcepts(entry);
        }

        static bool CheckOwnedEntityChange(EntityEntry entry)
        {
            return entry.State == EntityState.Modified ||
                   entry.References.Any(r =>
                       r.TargetEntry != null && r.TargetEntry.Metadata.IsOwned() && CheckOwnedEntityChange(r.TargetEntry));
        }
    }

    protected virtual void ApplyOpsConcepts(EntityEntry entry)
    {
        switch (entry.State)
        {
            case EntityState.Added:
                ApplyConceptsForAddedEntity(entry);
                break;
            case EntityState.Modified:
                ApplyConceptsForModifiedEntity(entry);
                break;
            case EntityState.Deleted:
                ApplyConceptsForDeletedEntity(entry);
                break;
        }
    }

    protected virtual void ApplyConceptsForAddedEntity(EntityEntry entry)
    {
        CheckIsGuidAndSet(entry);
        SetCreationAuditProperties(entry);
    }

    protected virtual void ApplyConceptsForModifiedEntity(EntityEntry entry)
    {
        SetModificationAuditProperties(entry);
    }

    protected virtual void ApplyConceptsForDeletedEntity(EntityEntry entry)
    {
        CancelDeletionForSoftDelete(entry);
    }

    protected void ConfigureGlobalFilters<TEntity>(ModelBuilder modelBuilder, IMutableEntityType mutableEntityType)
            where TEntity : class
    {
        if (ShouldFilterEntity<TEntity>())
        {
            var filterExpression = CreateFilterExpression<TEntity>();
            if (filterExpression != null)
            {
                modelBuilder.Entity<TEntity>().HasQueryFilter(filterExpression);
            }
        }
    }

    protected virtual bool ShouldFilterEntity<TEntity>() where TEntity : class
    {
        if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
        {
            return true;
        }

        return false;
    }

    protected virtual Expression<Func<TEntity, bool>>? CreateFilterExpression<TEntity>()
            where TEntity : class
    {
        Expression<Func<TEntity, bool>>? expression = null;

        if (typeof(ISoftDelete).IsAssignableFrom(typeof(TEntity)))
        {
            Expression<Func<TEntity, bool>> softDeleteFilter = e => !IsSoftDeleteFilterEnabled || !((ISoftDelete)e).IsDeleted;
            expression = expression == null ? softDeleteFilter : CombineExpressions(expression, softDeleteFilter);
        }

        return expression;
    }

    protected virtual void SetCreationAuditProperties(EntityEntry entry)
    {
        if (entry.Entity is IShouldHaveCreateTime entity0)
        {
            entity0.CreatedAt = DateTime.Now; // 考虑
        }
    }

    protected virtual void SetModificationAuditProperties(EntityEntry entry)
    {
        if (entry.Entity is IShouldHaveUpdateTime entity0)
        {
            entity0.UpdatedAt ??= DateTime.Now; // 考虑
        }
    }

    protected virtual void CancelDeletionForSoftDelete(EntityEntry entry)
    {
        if (entry.Entity is not ISoftDelete entity0)
        {
            return;
        }

        entry.Reload();
        entry.State = EntityState.Modified;
        entity0.IsDeleted = true;

        // 考虑更新时间
    }

    protected virtual void CheckIsGuidAndSet(EntityEntry entry)
    {
        if (entry.Entity is IEntity<Guid> entity)
        {
            var idPropertyEntry = entry.Property(nameof(entity.Id));
            if (idPropertyEntry != null && idPropertyEntry.Metadata.ValueGenerated == ValueGenerated.Never)
            {
                entity.Id = Guid.NewGuid();
            }
        }
    }

    protected virtual Expression<Func<T, bool>>? CombineExpressions<T>(Expression<Func<T, bool>> expression1, Expression<Func<T, bool>> expression2)
    {
        return ExpressionCombiner.Combine(expression1, expression2);
    }
}
