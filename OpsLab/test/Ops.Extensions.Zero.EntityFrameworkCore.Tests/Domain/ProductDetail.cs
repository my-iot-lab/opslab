using System.ComponentModel.DataAnnotations;
using Ops.Extensions.Zero.Domain.Entities.Auditing;

namespace Ops.Extensions.Zero.EntityFrameworkCore.Tests.Domain;

public class ProductDetail : AuditedEntity
{
    public Product Product { get; set; }

    [Required]
    public string Model { get; set; }
}
