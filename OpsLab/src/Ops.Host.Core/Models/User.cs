using Ops.Host.Common.Domain;

namespace Ops.Host.Core.Models;

/// <summary>
/// 用户信息
/// </summary>
public sealed class User : Entity
{
    /// <summary>
    /// 用户名
    /// </summary>
    [NotNull]
    public string? UserName { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [NotNull]
    public string? Password { get; set; }

    /// <summary>
    /// 显示名称
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
