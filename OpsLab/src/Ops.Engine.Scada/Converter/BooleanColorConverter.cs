using System;
using System.Globalization;
using System.Windows.Data;

namespace Ops.Engine.Scada.Converter;

/// <summary>
/// 颜色转换器，true -> Green; false -> Red。
/// </summary>
[ValueConversion(typeof(bool), typeof(string))]
internal sealed class BooleanColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? "Green" : "Red";
    }

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return null;
    }
}
