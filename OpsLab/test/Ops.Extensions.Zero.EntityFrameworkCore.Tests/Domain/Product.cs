using System.ComponentModel.DataAnnotations;
using Ops.Extensions.Zero.Domain.Entities;
using Ops.Extensions.Zero.Domain.Entities.Auditing;

namespace Ops.Extensions.Zero.EntityFrameworkCore.Tests.Domain;

public class Product : AuditedEntity, ISoftDelete
{
    [Required]
    public string Name { get; set; }

    public Status Status { get; set; }

    public bool IsDeleted { get; set; }

    public IList<ProductDetail> ProductDetails { get; set; }
}

public enum Status
{
    Active,
    Passive
}
