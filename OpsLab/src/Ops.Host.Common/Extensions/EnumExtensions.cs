using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace Ops.Host.Common.Extensions;

public static class EnumExtensions
{
    /// <summary>
    /// 获取指定类型的枚举字段列表。
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static List<NameValue<string, int>> ToDescriptionList(Type type)
    {
        List<NameValue<string, int>> list = new();
        foreach (var field in type.GetFields().Where(s => s.FieldType.IsEnum))
        {
            var attr = field.GetCustomAttribute<DescriptionAttribute>(false);
            list.Add(new NameValue<string, int>(attr?.Description ?? field.Name, (int)field.GetRawConstantValue()!));
        }

        return list;
    }

    public static string? Desc(this Enum source)
    {
        var fi = source.GetType().GetField(source.ToString());
        var attr = fi!.GetCustomAttribute<DescriptionAttribute>(false);
        return attr?.Description;
    }
}
