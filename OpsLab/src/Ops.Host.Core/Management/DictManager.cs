using Ops.Host.Common;

namespace Ops.Host.Core.Management;

/// <summary>
/// 字典信息管理者
/// </summary>
public sealed class DictManager
{
    /// <summary>
    /// 警报
    /// </summary>
    public const string Alarm = nameof(Alarm);

    /// <summary>
    /// 班次
    /// </summary>
    public const string Shift = nameof(Shift);

    /// <summary>
    /// 字典分类下拉框
    /// </summary>
    public static List<NameValue> CategoryDropdownList => new()
    {
        new NameValue("", ""),
        new NameValue("警报", Alarm),
        new NameValue("班次", Shift),
    };
}
