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
}
