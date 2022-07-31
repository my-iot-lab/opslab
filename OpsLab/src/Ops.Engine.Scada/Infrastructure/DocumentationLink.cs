﻿using System;
using System.Windows.Controls;
using System.Windows.Input;
using Ops.Engine.Scada.Utils;

namespace Ops.Engine.Scada.Infrastructure;

/// <summary>
/// 文档超链接
/// </summary>
public class DocumentationLink
{
    public DocumentationLink(DocumentationLinkType type, string url)
            : this(type, url, null)
    {
    }

    public DocumentationLink(DocumentationLinkType type, string url, string? label)
    {
        Label = label ?? type.ToString();
        Url = url;
        Type = type;
        Open = new AnotherCommandImplementation(Execute);
    }

    public static DocumentationLink WikiLink(string page, string label)
    {
        return new DocumentationLink(DocumentationLinkType.Wiki,
            $"https://github.com/ButchersBoy/MaterialDesignInXamlToolkit/wiki/" + page, label);
    }

    public static DocumentationLink StyleLink(string nameChunk)
    {
        return new DocumentationLink(
            DocumentationLinkType.StyleSource,
            $"https://github.com/ButchersBoy/MaterialDesignInXamlToolkit/blob/master/MaterialDesignThemes.Wpf/Themes/MaterialDesignTheme.{nameChunk}.xaml",
            nameChunk);
    }

    public static DocumentationLink ApiLink<TClass>(string subNamespace)
    {
        var typeName = typeof(TClass).Name;

        return new DocumentationLink(
            DocumentationLinkType.ControlSource,
            $"https://github.com/ButchersBoy/MaterialDesignInXamlToolkit/blob/master/MaterialDesignThemes.Wpf/{subNamespace}/{typeName}.cs",
            typeName);
    }


    public static DocumentationLink ApiLink<TClass>()
        => ApiLink(typeof(TClass));

    public static DocumentationLink ApiLink(Type type)
    {
        var typeName = type.Name;

        return new DocumentationLink(
            DocumentationLinkType.ControlSource,
            $"https://github.com/ButchersBoy/MaterialDesignInXamlToolkit/blob/master/MaterialDesignThemes.Wpf/{typeName}.cs",
            typeName);
    }

    public static DocumentationLink DemoPageLink<TDemoPage>()
        => DemoPageLink<TDemoPage>(null);

    public static DocumentationLink DemoPageLink<TDemoPage>(string? label)
        => DemoPageLink<TDemoPage>(label, null);

    public static DocumentationLink DemoPageLink<TDemoPage>(string? label, string? @namespace)
    {
        var ext = typeof(UserControl).IsAssignableFrom(typeof(TDemoPage))
            ? "xaml"
            : "cs";


        return new DocumentationLink(
            DocumentationLinkType.DemoPageSource,
            $"https://github.com/ButchersBoy/MaterialDesignInXamlToolkit/blob/master/MainDemo.Wpf/{(string.IsNullOrWhiteSpace(@namespace) ? "" : "/" + @namespace + "/")}{typeof(TDemoPage).Name}.{ext}",
            label ?? typeof(TDemoPage).Name);
    }

    public string Label { get; }

    public string Url { get; }

    public DocumentationLinkType Type { get; }

    public ICommand Open { get; }

    private void Execute(object? _)
    {
        Link.OpenInBrowser(Url);
    }
}

/// <summary>
/// 文档连接类型
/// </summary>
public enum DocumentationLinkType
{
    Wiki,
    DemoPageSource,
    ControlSource,
    StyleSource,
    Video,
}