using Ops.Exchange.Model;

namespace Ops.Exchange.Forwarder;

public static class ForwardDataExtensions
{
    /// <summary>
    /// 获取相应的负载数据。
    /// <para>通知事件包含触发信号本身本身，响应事件不包含触发信号本身数据。</para>
    /// </summary>
    /// <param name="tag">tag 标签</param>
    /// <returns></returns>
    public static PayloadData? GetPayloadData(this ForwardData forward, string tag)
    {
        return forward.Values.FirstOrDefault(s => string.Equals(s.Tag, tag, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 是否存在相应的标签。
    /// <para>通知事件包含触发信号本身本身，响应事件不包含触发信号本身数据。</para>
    /// </summary>
    /// <param name="tag">tag 标签</param>
    /// <returns></returns>
    public static bool Exists(this ForwardData forward, string tag)
    {
        return forward.Values.Any(s => string.Equals(s.Tag, tag, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// 获取自身数据，只有通知事件和Underly事件包含触发信号本身数据。
    /// </summary>
    /// <param name="forward"></param>
    /// <returns></returns>
    public static PayloadData Self(this ForwardData forward)
    {
        return forward.Values[0]; // 通知事件只有一个值，Underly 事件自身值在起始位置。
    }

    /// <summary>
    /// 获取相应的对象值，没有找到对象则返回 default。
    /// 若对象类型不能转换，会抛出异常。
    /// <para>注：若原始数据为数组，指定类型为 string，会将数组转换为 string，值以逗号隔开。</para>
    /// <para>通知事件包含触发信号本身；响应事件不包含触发信号本身。</para>
    /// </summary>
    /// <param name="tag">tag 标签</param>
    /// <returns></returns>
    public static T? GetValue<T>(this ForwardData forward, string tag)
    {
        var payload = forward.GetPayloadData(tag);
        if (payload == null)
        {
            return default;
        }

        return payload.GetValue<T>();
    }

    /// <summary>
    /// 获取指定 Tag 的负载数据值，值类型为 <see cref="VariableType.Bit"/>。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// <para>通知事件包含触发信号本身本身，响应事件不包含触发信号本身数据。</para>
    /// </summary>
    /// <param name="tag">tag 标签</param>
    /// <returns></returns>
    public static bool? GetBit(this ForwardData forward, string tag)
    {
        var payload = forward.GetPayloadData(tag);
        return payload != null && payload.GetBit();
    }

    /// <summary>
    /// 获取指定 Tag 的负载数据数组集合，值类型为 <see cref="VariableType.Bit"/>。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// <para>通知事件包含触发信号本身本身，响应事件不包含触发信号本身数据。</para>
    /// </summary>
    /// <param name="tag">tag 标签</param>
    /// <returns></returns>
    public static bool[]? GetBitArray(this ForwardData forward, string tag)
    {
        var payload = forward.GetPayloadData(tag);
        return payload != null ? payload.GetBitArray() : default;
    }

    /// <summary>
    /// 获取指定 Tag 的负载数据值，值类型为 <see cref="VariableType.Byte"/>。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// <para>通知事件包含触发信号本身本身，响应事件不包含触发信号本身数据。</para>
    /// </summary>
    /// <param name="tag">tag 标签</param>
    /// <returns></returns>
    public static byte? GetByte(this ForwardData forward, string tag)
    {
        var payload = forward.GetPayloadData(tag);
        return payload != null ? payload.GetByte() : default;
    }

    /// <summary>
    /// 获取指定 Tag 的负载数据数组集合，值类型为 <see cref="VariableType.Byte"/>。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// <para>通知事件包含触发信号本身本身，响应事件不包含触发信号本身数据。</para>
    /// </summary>
    /// <param name="tag">tag 标签</param>
    /// <returns></returns>
    public static byte[]? GetByteArray(this ForwardData forward, string tag)
    {
        var payload = forward.GetPayloadData(tag);
        return payload != null ? payload.GetByteArray() : default;
    }

    /// <summary>
    /// 获取指定 Tag 的负载数据值，值类型为 <see cref="VariableType.Word"/>。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// <para>通知事件包含触发信号本身本身，响应事件不包含触发信号本身数据。</para>
    /// </summary>
    /// <param name="tag">tag 标签</param>
    /// <returns></returns>
    public static ushort? GetWord(this ForwardData forward, string tag)
    {
        var payload = forward.GetPayloadData(tag);
        return payload != null ? payload.GetWord() : default;
    }

    /// <summary>
    /// 获取指定 Tag 的负载数据数组集合，值类型为 <see cref="VariableType.Word"/>。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// <para>通知事件包含触发信号本身本身，响应事件不包含触发信号本身数据。</para>
    /// </summary>
    /// <param name="tag">tag 标签</param>
    /// <returns></returns>
    public static ushort[]? GetWordArray(this ForwardData forward, string tag)
    {
        var payload = forward.GetPayloadData(tag);
        return payload != null ? payload.GetWordArray() : default;
    }

    /// <summary>
    /// 获取指定 Tag 的负载数据值，值类型为 <see cref="VariableType.DWord"/>。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// <para>通知事件包含触发信号本身本身，响应事件不包含触发信号本身数据。</para>
    /// </summary>
    /// <param name="tag">tag 标签</param>
    /// <returns></returns>
    public static uint? GetDWord(this ForwardData forward, string tag)
    {
        var payload = forward.GetPayloadData(tag);
        return payload != null ? payload.GetDWord() : default;
    }

    /// <summary>
    /// 获取指定 Tag 的负载数据数组集合，值类型为 <see cref="VariableType.DWord"/>。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// <para>通知事件包含触发信号本身本身，响应事件不包含触发信号本身数据。</para>
    /// </summary>
    /// <param name="tag">tag 标签</param>
    /// <returns></returns>
    public static uint[]? GetDWordArray(this ForwardData forward, string tag)
    {
        var payload = forward.GetPayloadData(tag);
        return payload != null ? payload.GetDWordArray() : default;
    }

    /// <summary>
    /// 获取指定 Tag 的负载数据值，值类型为 <see cref="VariableType.Int"/>。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// <para>通知事件包含触发信号本身本身，响应事件不包含触发信号本身数据。</para>
    /// </summary>
    /// <param name="tag">tag 标签</param>
    /// <returns></returns>
    public static short? GetInt(this ForwardData forward, string tag)
    {
        var payload = forward.GetPayloadData(tag);
        return payload != null ? payload.GetInt() : default;
    }

    /// <summary>
    /// 获取指定 Tag 的负载数据数组集合，值类型为 <see cref="VariableType.Int"/>。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// <para>通知事件包含触发信号本身本身，响应事件不包含触发信号本身数据。</para>
    /// </summary>
    /// <param name="tag">tag 标签</param>
    /// <returns></returns>
    public static short[]? GetIntArray(this ForwardData forward, string tag)
    {
        var payload = forward.GetPayloadData(tag);
        return payload != null ? payload.GetIntArray() : default;
    }

    /// <summary>
    /// 获取指定 Tag 的负载数据值，值类型为 <see cref="VariableType.DInt"/>。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// <para>通知事件包含触发信号本身本身，响应事件不包含触发信号本身数据。</para>
    /// </summary>
    /// <param name="tag">tag 标签</param>
    /// <returns></returns>
    public static int? GetDInt(this ForwardData forward, string tag)
    {
        var payload = forward.GetPayloadData(tag);
        return payload != null ? payload.GetDInt() : default;
    }

    /// <summary>
    /// 获取指定 Tag 的负载数据数组集合，值类型为 <see cref="VariableType.DInt"/>。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// <para>通知事件包含触发信号本身本身，响应事件不包含触发信号本身数据。</para>
    /// </summary>
    /// <param name="tag">tag 标签</param>
    /// <returns></returns>
    public static int[]? GetDIntArray(this ForwardData forward, string tag)
    {
        var payload = forward.GetPayloadData(tag);
        return payload != null ? payload.GetDIntArray() : default;
    }

    /// <summary>
    /// 获取指定 Tag 的负载数据值，值类型为 <see cref="VariableType.Real"/>。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// <para>通知事件包含触发信号本身本身，响应事件不包含触发信号本身数据。</para>
    /// </summary>
    /// <param name="tag">tag 标签</param>
    /// <returns></returns>
    public static float? GetReal(this ForwardData forward, string tag)
    {
        var payload = forward.GetPayloadData(tag);
        return payload != null ? payload.GetReal() : default;
    }

    /// <summary>
    /// 获取指定 Tag 的负载数据数组集合，值类型为 <see cref="VariableType.Real"/>。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// <para>通知事件包含触发信号本身本身，响应事件不包含触发信号本身数据。</para>
    /// </summary>
    /// <param name="tag">tag 标签</param>
    /// <returns></returns>
    public static float[]? GetRealArray(this ForwardData forward, string tag)
    {
        var payload = forward.GetPayloadData(tag);
        return payload != null ? payload.GetRealArray() : default;
    }

    /// <summary>
    /// 获取指定 Tag 的负载数据值，值类型为 <see cref="VariableType.LReal"/>。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// <para>通知事件包含触发信号本身本身，响应事件不包含触发信号本身数据。</para>
    /// </summary>
    /// <param name="tag">tag 标签</param>
    /// <returns></returns>
    public static double? GetLReal(this ForwardData forward, string tag)
    {
        var payload = forward.GetPayloadData(tag);
        return payload != null ? payload.GetLReal() : default;
    }

    /// <summary>
    /// 获取指定 Tag 的负载数据数组集合，值类型为 <see cref="VariableType.LReal"/>。
    /// 取值时给定的类型必须与实际类型一致，期间会进行强制转换。若对象类型不能转换，会抛出异常。
    /// <para>通知事件包含触发信号本身本身，响应事件不包含触发信号本身数据。</para>
    /// </summary>
    /// <param name="tag">tag 标签</param>
    /// <returns></returns>
    public static double[]? GetLRealArray(this ForwardData forward, string tag)
    {
        var payload = forward.GetPayloadData(tag);
        return payload != null ? payload.GetLRealArray() : default;
    }

    /// <summary>
    /// 获取 <see cref="VariableType.String"/> 或 <see cref="VariableType.S7String"/> 或 <see cref="VariableType.S7WString"/>类型的值。
    /// 此方法与调用 <code>GetValue<string>()</string></code> 一样。
    /// </summary>
    /// <returns></returns>
    public static string? GetString(this ForwardData forward, string tag)
    {
        var payload = forward.GetPayloadData(tag);
        return payload != null ? payload.GetString() : default;
    }
}
