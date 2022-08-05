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
    /// <remarks>
    /// Name 显示 <see cref="DisplayNameAttribute"/> 描述，若没有，这显示 <see cref="DescriptionAttribute"/>，再没有则显示字段名称；Value 为字段值（int 类型）。
    /// </remarks>
    /// <returns></returns>
    public static List<NameValue<string, int>> ToNameValueList<T>()
         where T : Enum
    {
        var fields = typeof(T).GetFields().Where(s => s.FieldType.IsEnum).ToList();
        List<NameValue<string, int>> list = new(fields.Count);

        foreach (var field in fields)
        {
            var attr0 = field.GetCustomAttribute<DisplayNameAttribute>(false);
            if (attr0 != null)
            {
                list.Add(new NameValue<string, int>(attr0!.DisplayName, (int)field.GetRawConstantValue()!));
                continue;
            }

            var attr1 = field.GetCustomAttribute<DescriptionAttribute>(false);
            list.Add(new NameValue<string, int>(attr1?.Description ?? field.Name, (int)field.GetRawConstantValue()!));
        }
        
        return list;
    }

    /// <summary>
    /// 获取指定类型的枚举字段列表。
    /// </summary>
    /// <remarks>
    /// Name 显示 <see cref="DisplayNameAttribute"/> 描述，若没有，这显示 <see cref="DescriptionAttribute"/>，再没有则显示字段名称；Value 为字段名称。
    /// </remarks>
    /// <returns></returns>
    public static List<NameValue> ToNameValueList2<T>()
         where T : Enum
    {
        var fields = typeof(T).GetFields().Where(s => s.FieldType.IsEnum).ToList();
        List<NameValue> list = new(fields.Count);

        foreach (var field in fields)
        {
            var attr0 = field.GetCustomAttribute<DisplayNameAttribute>(false);
            if (attr0 != null)
            {
                list.Add(new NameValue(attr0!.DisplayName, field.Name));
                continue;
            }

            var attr1 = field.GetCustomAttribute<DescriptionAttribute>(false);
            list.Add(new NameValue(attr1?.Description ?? field.Name, field.Name));
        }

        return list;
    }

    /// <summary>
    /// 获取枚举类型的 <see cref="DescriptionAttribute"/> 描述，没有则为 null。
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public static string? Desc(this Enum source)
    {
        var fi = source.GetType().GetField(source.ToString());
        var attr = fi!.GetCustomAttribute<DescriptionAttribute>(false);
        return attr?.Description;
    }
}
