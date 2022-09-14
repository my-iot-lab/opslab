namespace Ops.Exchange.Model;

public static class PayloadDataExtensions
{
    /// <summary>
    /// 拼接数组的分隔符。
    /// </summary>
    public const char ArraySeparatorChar = ',';

    /// <summary>
    /// 获取相应的对象值，没有找到对象则为 default。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// <para>注：若原始数据为数组，目标类型为 string，会将数组转换为 string，值以逗号隔开。</para>
    /// </summary>
    /// <returns></returns>
    public static T? GetValue<T>(this PayloadData payload)
    {
        if (payload.Value == null)
        {
            return default;
        }

        // 若不是数组
        if (!payload.Value.GetType().IsArray)
        {
            // 若是目标类型为字符串
            if (typeof(T) == typeof(string))
            {
                object? obj = payload.Value.ToString();
                return (T?)obj;
            }

            // 强转
            return (T?)payload.Value;
        }

        // 数组处理
        // 若是目标类型为字符串，将数组拼接为字符串。
        if (typeof(T) == typeof(string))
        {
            object obj = payload.VarType switch
            {
                VariableType.Bit => string.Join(ArraySeparatorChar, (bool[])payload.Value),
                VariableType.Byte => string.Join(ArraySeparatorChar, (byte[])payload.Value),
                VariableType.Word => string.Join(ArraySeparatorChar, (ushort[])payload.Value),
                VariableType.DWord => string.Join(ArraySeparatorChar, (uint[])payload.Value),
                VariableType.Int => string.Join(ArraySeparatorChar, (short[])payload.Value),
                VariableType.DInt => string.Join(ArraySeparatorChar, (int[])payload.Value),
                VariableType.Real => string.Join(ArraySeparatorChar, (float[])payload.Value),
                VariableType.LReal => string.Join(ArraySeparatorChar, (double[])payload.Value),
                VariableType.String or VariableType.S7String or VariableType.S7WString => string.Join(",", (string[])payload.Value),
                _ => throw new NotImplementedException(),
            };

            return (T)obj;
        }

        // 若想返回类型为数组，强制转换为对应类型。
        if (typeof(T).IsArray)
        {
            return (T?)payload.Value;
        }

        return default;
    }

    /// <summary>
    /// 获取 <see cref="VariableType.Bit"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    public static bool GetBit(this PayloadData payload)
    {
        return payload.GetValue<bool>();
    }

    /// <summary>
    /// 获取 <see cref="VariableType.Bit"/> 类型的数组集合。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    public static bool[]? GetBitArray(this PayloadData payload)
    {
        return payload.GetValue<bool[]>();
    }

    /// <summary>
    /// 获取 <see cref="VariableType.Byte"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    public static byte GetByte(this PayloadData payload)
    {
        return payload.GetValue<byte>();
    }

    /// <summary>
    /// 获取 <see cref="VariableType.Byte"/> 类型的数组集合。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    public static byte[]? GetByteArray(this PayloadData payload)
    {
        return payload.GetValue<byte[]>();
    }

    /// <summary>
    /// 获取 <see cref="VariableType.Word"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    public static ushort GetWord(this PayloadData payload)
    {
        return payload.GetValue<ushort>();
    }

    /// <summary>
    /// 获取 <see cref="VariableType.Word"/> 类型的数组集合。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    public static ushort[]? GetWordArray(this PayloadData payload)
    {
        return payload.GetValue<ushort[]>();
    }

    /// <summary>
    /// 获取 <see cref="VariableType.DWord"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    public static uint GetDWord(this PayloadData payload)
    {
        return payload.GetValue<uint>();
    }

    /// <summary>
    /// 获取 <see cref="VariableType.DWord"/> 类型的数组集合。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    public static uint[]? GetDWordArray(this PayloadData payload)
    {
        return payload.GetValue<uint[]>();
    }

    /// <summary>
    /// 获取 <see cref="VariableType.Int"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    public static short GetInt(this PayloadData payload)
    {
        return payload.GetValue<short>();
    }

    /// <summary>
    /// 获取 <see cref="VariableType.Int"/> 类型的数组集合。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    public static short[]? GetIntArray(this PayloadData payload)
    {
        return payload.GetValue<short[]>();
    }

    /// <summary>
    /// 获取 <see cref="VariableType.DInt"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    public static int GetDInt(this PayloadData payload)
    {
        return payload.GetValue<int>();
    }

    /// <summary>
    /// 获取 <see cref="VariableType.DInt"/> 类型的数组集合。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    public static int[]? GetDIntArray(this PayloadData payload)
    {
        return payload.GetValue<int[]>();
    }

    /// <summary>
    /// 获取 <see cref="VariableType.Real"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    public static float GetReal(this PayloadData payload)
    {
        return payload.GetValue<float>();
    }

    /// <summary>
    /// 获取 <see cref="VariableType.Real"/> 类型的数组集合。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    public static float[]? GetRealArray(this PayloadData payload)
    {
        return payload.GetValue<float[]>();
    }

    /// <summary>
    /// 获取 <see cref="VariableType.LReal"/> 类型的值。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    public static double GetLReal(this PayloadData payload)
    {
        return payload.GetValue<double>();
    }

    /// <summary>
    /// 获取 <see cref="VariableType.LReal"/> 类型的数组集合。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// </summary>
    /// <returns></returns>
    public static double[]? GetLRealArray(this PayloadData payload)
    {
        return payload.GetValue<double[]>();
    }

    /// <summary>
    /// 获取 <see cref="VariableType.String"/> 或 <see cref="VariableType.S7String"/> 或 <see cref="VariableType.S7WString"/>类型的值。
    /// 此方法与调用 <code>GetValue<string>()</string></code> 一样。
    /// </summary>
    /// <returns></returns>
    public static string? GetString(this PayloadData payload)
    {
        return payload.GetValue<string>();
    }
}
