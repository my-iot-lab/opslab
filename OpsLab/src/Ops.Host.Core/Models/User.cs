using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ops.Host.Core.Models;

/// <summary>
/// 用户信息
/// </summary>
[Table("User")]
public sealed class User : Entity
{
    /// <summary>
    /// 用户名
    /// </summary>
    [NotNull]
    [DisplayName("用户名")]
    public string? UserName { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [NotNull]
    [DisplayName("密码")]
    public string? Password { get; set; }

    /// <summary>
    /// 显示名
    /// </summary>
    [DisplayName("显示名")]
    public string? DisplayName { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    [DisplayName("创建时间")]
    public DateTime CreatedAt { get; set; }
}
