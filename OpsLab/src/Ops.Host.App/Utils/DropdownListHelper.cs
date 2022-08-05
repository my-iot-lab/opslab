using System;
using System.Collections.Generic;
using System.Linq;
using Ops.Host.Common;
using Ops.Host.Common.Extensions;

namespace Ops.Host.App.Utils;

public static class DropdownListHelper
{
    /// <summary>
    /// 将枚举类型转换为下拉框选项。
    /// </summary>
    /// <param name="enumType">枚举类型</param>
    /// <returns></returns>
    public static List<NameValue> FromEnum<T>()
        where T : Enum
    {
        var items = EnumExtensions.ToNameValueList2<T>();
        items.Insert(0, new NameValue("", ""));

        return items;
    }

    /// <summary>
    /// 将字符串数组换为下拉框选项。
    /// </summary>
    /// <param name="strArray">字符串数组</param>
    /// <returns></returns>
    public static List<string> FromString(IEnumerable<string> strArray)
    {
        var items = new List<string>(strArray.Count() + 1)
        {
            "",
        };

        items.AddRange(strArray);

        return items;
    }
}
