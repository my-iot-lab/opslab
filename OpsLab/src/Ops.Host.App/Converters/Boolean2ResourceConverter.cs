using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Ops.Host.App.Converters;

/// <summary>
/// Bool 类型转换为指定的资源
/// </summary>
public sealed class Boolean2ResourceConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        FrameworkElement element = new();
        if (value is bool v)
        {
            if (parameter is string text)
            {
                string resourceKey = "";
                string[] array = text.Split(';');
                if (array.Length > 1)
                {
                    resourceKey = !v ? array[0] : array[1];
                }

                return element.TryFindResource(resourceKey);
            }

            return new();
        }

        return new();
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
