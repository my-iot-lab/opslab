using System;
using System.Globalization;
using System.Windows.Data;

namespace Ops.Engine.UI.Converter;

/// <summary>
/// 连接状态描述转换器，true -> 已连接; false -> 已断开。
/// </summary>
[ValueConversion(typeof(bool), typeof(string))]
internal class BooleanConnectionDescConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? "已连接" : "已断开";
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}
