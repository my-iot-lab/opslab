using System.ComponentModel;

namespace BootstrapAdmin.DataAccess.Models;

/// <summary>
/// 资源类型枚举 0 表示菜单 1 表示资源 2 表示按钮
/// </summary>
public enum EnumResource
{
    [Description("菜单")]
    Navigation,

    [Description("资源")]
    Resource,

    [Description("代码块")]
    Block
}
