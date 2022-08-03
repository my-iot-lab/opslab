using Ops.Host.App.Models;
using Ops.Host.App.UserControls;

namespace Ops.Host.App.Management;

/// <summary>
/// 左侧菜单管理
/// </summary>
public sealed class MenuManager
{
    /// <summary>
    /// 获取菜单
    /// </summary>
    public static MenuItemModel[] Menus => new[]
    {
        new MenuItemModel("", "首页", typeof(Home), true),
        new MenuItemModel("", "Demo1", typeof(Demo1)),
        new MenuItemModel("", "首页3", null),
    };
}
