using System.Text.Json;

namespace BootstrapAdmin.Web.Utils;

/// <summary>
/// 将对象转换为具体的值
/// </summary>
public static class Object2ValueHelper
{
    /// <summary>
    /// 将对象转换为指定类型的基元对象。
    /// 注意：对象可为传统的基元对象，或是 JSON 反序列化的 <see cref="JsonElement"/> 对象。
    /// </summary>
    /// <typeparam name="T">要转换的基元类型</typeparam>
    /// <param name="obj">对象</param>
    /// <returns></returns>
    public static T To<T>(object obj)
    {
        if (obj is JsonElement jsonElement)
        {
            return JsonObjectTo<T>(jsonElement);
        }

        return ObjectTo<T>(obj);
    }

    /// <summary>
    /// 将对象转换为指定类型的基元对象数组。
    /// 注意：对象可为原始的基元对象，或是 JSON 反序列化的 <see cref="JsonElement"/> 对象。
    /// </summary>
    /// <typeparam name="T">要转换的类型</typeparam>
    /// <param name="obj">对象</param>
    /// <returns></returns>
    public static T[] ToArray<T>(object obj)
    {
        if (obj is JsonElement jsonElement)
        {
            return JsonObject2Array<T>(jsonElement);
        }

        return ObjectToArray<T>(obj);
    }

    /// <summary>
    /// 将对象转换为指定类型的基元对象数组。
    /// </summary>
    /// <typeparam name="T">要转换的基元类型</typeparam>
    /// <param name="obj">对象</param>
    /// <returns></returns>
    public static T[] ObjectToArray<T>(object obj)
    {
        if (obj is T[] obj2)
        {
            return obj2;
        }

        if (!obj.GetType().IsArray)
        {
            return Array.Empty<T>();
        }

        if (typeof(T) == typeof(byte))
        {
            var arr = (int[])obj;
            return arr.Select(s => Convert.ToByte(s)).Cast<T>().ToArray();
        }
        if (typeof(T) == typeof(bool))
        {
            var arr = (bool[])obj;
            return arr.Cast<T>().ToArray();
        }
        if (typeof(T) == typeof(ushort))
        {
            var arr = (int[])obj;
            return arr.Select(s => Convert.ToUInt16(s)).Cast<T>().ToArray();
        }
        if (typeof(T) == typeof(short))
        {
            var arr = (int[])obj;
            return arr.Select(s => Convert.ToInt16(s)).Cast<T>().ToArray();
        }
        if (typeof(T) == typeof(uint))
        {
            var arr = (int[])obj;
            return arr.Select(s => Convert.ToUInt32(s)).Cast<T>().ToArray();
        }
        if (typeof(T) == typeof(int))
        {
            var arr = (int[])obj;
            return arr.Select(s => Convert.ToInt32(s)).OfType<T>().ToArray();
        }
        if (typeof(T) == typeof(float))
        {
            var arr = (float[])obj;
            return arr.Select(s => Convert.ToSingle(s)).Cast<T>().ToArray();
        }
        if (typeof(T) == typeof(double))
        {
            var arr = (double[])obj;
            return arr.Select(s => Convert.ToDouble(s)).Cast<T>().ToArray();
        }
        if (typeof(T) == typeof(string))
        {
            var arr = (string[])obj;
            return arr.Cast<T>().ToArray();
        }

        return Array.Empty<T>();
    }

    /// <summary>
    /// 将对象转换为指定类型的基元对象。
    /// </summary>
    /// <typeparam name="T">要转换的基元类型</typeparam>
    /// <param name="obj">对象</param>
    /// <returns></returns>
    public static T ObjectTo<T>(object obj)
    {
        object? obj2 = null;

        if (typeof(T) == typeof(byte))
        {
            obj2 = Convert.ToByte(obj);
        }
        else if (typeof(T) == typeof(bool))
        {
            obj2 = Convert.ToBoolean(obj);
        }
        else if (typeof(T) == typeof(ushort))
        {
            obj2 = Convert.ToUInt16(obj);
        }
        else if (typeof(T) == typeof(short))
        {
            obj2 = Convert.ToInt16(obj);
        }
        else if (typeof(T) == typeof(uint))
        {
            obj2 = Convert.ToUInt32(obj);
        }
        else if (typeof(T) == typeof(int))
        {
            obj2 = Convert.ToInt32(obj);
        }
        else if (typeof(T) == typeof(float))
        {
            obj2 = Convert.ToSingle(obj);
        }
        else if (typeof(T) == typeof(double))
        {
            obj2 = Convert.ToDouble(obj);
        }
        else if (typeof(T) == typeof(string))
        {
            obj2 = Convert.ToString(obj);
        }

        return (T)obj2;
    }

    /// <summary>
    /// 将 JSON 反序列化的对象转换为指定类型的基元对象数组。
    /// </summary>
    /// <typeparam name="T">要转换的基元类型</typeparam>
    /// <param name="jsonElement">Json 元素</param>
    /// <returns></returns>
    private static T[] JsonObject2Array<T>(JsonElement jsonElement)
    {
        if (jsonElement.ValueKind == JsonValueKind.Array)
        {
            var len = jsonElement.GetArrayLength();
            var arr = new List<T>(len);
            foreach (var item in jsonElement.EnumerateArray())
            {
                T obj = JsonObjectTo<T>(item);
                arr.Add(obj);
            }

            return arr.ToArray();
        }

        return Array.Empty<T>();
    }

    /// <summary>
    /// 将 JSON 反序列化的对象转换为指定类型的基元对象。
    /// </summary>
    /// <typeparam name="T">要转换的基元类型</typeparam>
    /// <param name="jsonElement">Json 元素</param>
    /// <returns></returns>
    public static T JsonObjectTo<T>(JsonElement jsonElement)
    {
        object? obj = null;
        if (jsonElement.ValueKind == JsonValueKind.True)
        {
            obj = true;
        }
        else if (jsonElement.ValueKind == JsonValueKind.False)
        {
            obj = false;
        }
        else if (jsonElement.ValueKind == JsonValueKind.Number)
        {
            if (typeof(T) == typeof(byte))
            {
                obj = jsonElement.GetByte();
            }
            else if (typeof(T) == typeof(sbyte))
            {
                obj = jsonElement.GetSByte();
            }
            else if (typeof(T) == typeof(ushort))
            {
                obj = jsonElement.GetUInt16();
            }
            else if (typeof(T) == typeof(short))
            {
                obj = jsonElement.GetInt16();
            }
            else if (typeof(T) == typeof(uint))
            {
                obj = jsonElement.GetUInt32();
            }
            else if (typeof(T) == typeof(int))
            {
                obj = jsonElement.GetInt32();
            }
            else if (typeof(T) == typeof(float))
            {
                obj = jsonElement.GetSingle();
            }
            else if (typeof(T) == typeof(double))
            {
                obj = jsonElement.GetDouble();
            }
        }
        else if (jsonElement.ValueKind == JsonValueKind.String)
        {
            if (typeof(T) == typeof(string))
            {
                obj = jsonElement.GetString();
            }
        }

        return (T)obj;
    }
}
