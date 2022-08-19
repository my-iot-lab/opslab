namespace Ops.Host.Core.Models;

public sealed class UserFilter
{
    public string? UserName { get; set; }

    public DateTime? CreatedAtStart { get; set; }

    public DateTime? CreatedAtEnd { get; set; }
}
