namespace Ops.Exchange.Model;

/// <summary>
/// 要传输的数据上下文对象。
/// </summary>
public sealed class PayloadContext
{
    /// <summary>
    /// 从设备中读取的数据。
    /// </summary>
    public PayloadRequest Request { get; }

    /// <summary>
    /// 应用程序端数据，要写入到设备中。
    /// </summary>
    public PayloadResponse Response { get; }

    public PayloadContext(PayloadRequest request)
    {
        Request = request;
        Response = new();
    }

    /// <summary>
    /// 设置响应的值。
    /// <para>要响应的值的 Tag 必须有在设备变量中设置，且设置的值需显示使用目标类型，否则强制转换可能失败。</para>
    /// </summary>
    /// <param name="tag">要设置值的 Tag。</param>
    /// <param name="value">要设置的值，注意，要显示使用目标类型的值。</param>
    public void SetResponseValue(string tag, object value)
    {
        var variable = Request.DeviceInfo.GetVariable(tag);
        if (variable != null)
        {
            Response.Values.Add(PayloadData.From(variable!, value));
        }
    }
}
