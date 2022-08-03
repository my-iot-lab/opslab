using System;

namespace Ops.Host.App.Models;

public sealed class MenuItemModel
{
    /// <summary>
    /// 菜单图标
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 菜单名称
    /// </summary>
    public string? Name { get; set; }

    public Type? ContentType { get; set; }

    public object? Content { get; set; }

    /// <summary>
    /// 是否为首页
    /// </summary>
    public bool IsHome { get; set; }

    public MenuItemModel()
    {

    }

    public MenuItemModel(string icon, string name, Type? contentType, bool isHome = false)
    {
        Icon = icon;
        Name = name;
        ContentType = contentType;
        IsHome = isHome;
    }
}
