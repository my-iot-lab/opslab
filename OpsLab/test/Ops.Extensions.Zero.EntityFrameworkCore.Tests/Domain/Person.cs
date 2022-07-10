using Ops.Extensions.Zero.Domain.Entities;

namespace Ops.Extensions.Zero.EntityFrameworkCore.Tests.Domain;

public class Person : Entity
{
    public string Name { get; set; }
}
