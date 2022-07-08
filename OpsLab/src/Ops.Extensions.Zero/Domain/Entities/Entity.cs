﻿namespace Ops.Extensions.Zero.Domain.Entities;

public abstract class Entity<TPrimaryKey> : IEntity<TPrimaryKey>
{
    public virtual TPrimaryKey Id { get; set; }

    public virtual bool IsTransient()
    {
        if (EqualityComparer<TPrimaryKey>.Default.Equals(Id, default(TPrimaryKey)))
        {
            return true;
        }

        //Workaround for EF Core since it sets int/long to min value when attaching to dbcontext
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

public abstract class Entity : Entity<int>, IEntity
{
}
