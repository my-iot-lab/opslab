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

    public MenuItemModel(string icon, string name, Type? contentType)
    {
        Icon = icon;
        Name = name;
        ContentType = contentType;
    }
}
