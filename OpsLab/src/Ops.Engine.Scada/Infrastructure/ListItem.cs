using System;
using System.Windows;
using System.Windows.Controls;

namespace Ops.Engine.Scada.Infrastructure;

public class ListItem : ViewModelBase
{
    private readonly Type _contentType;
    private readonly object? _dataContext;

    private object? _content;
    private ScrollBarVisibility _horizontalScrollBarVisibilityRequirement;
    private ScrollBarVisibility _verticalScrollBarVisibilityRequirement = ScrollBarVisibility.Auto;
    private Thickness _marginRequirement = new(16);

    public ListItem(string name, Type contentType, object? dataContext = null)
    {
        Name = name;
        _contentType = contentType;
        _dataContext = dataContext;
    }

    /// <summary>
    /// 获取其名称。
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 获取 Item 的内容。
    /// <para>
    /// 若初始化时 "dataContext" 不为 null，且 "contentType" 为 <see cref="FrameworkElement"/>，
    /// 会将其 DataContext 属性设置 "dataContext" 值。
    /// </para>
    /// </summary>
    public object? Content => _content ??= CreateContent();

    public ScrollBarVisibility HorizontalScrollBarVisibilityRequirement
    {
        get => _horizontalScrollBarVisibilityRequirement;
        set => SetProperty(ref _horizontalScrollBarVisibilityRequirement, value);
    }

    public ScrollBarVisibility VerticalScrollBarVisibilityRequirement
    {
        get => _verticalScrollBarVisibilityRequirement;
        set => SetProperty(ref _verticalScrollBarVisibilityRequirement, value);
    }

    public Thickness MarginRequirement
    {
        get => _marginRequirement;
        set => SetProperty(ref _marginRequirement, value);
    }

    private object? CreateContent()
    {
        var content = Activator.CreateInstance(_contentType);
        if (_dataContext != null && content is FrameworkElement element)
        {
            element.DataContext = _dataContext;
        }

        return content;
    }
}
