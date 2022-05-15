namespace Ops.Exchange.Utils;

/// <summary>
/// GUID 唯一码生成器。
/// </summary>
internal sealed class GuidIdGenerator
{
    /// <summary>
    /// 32 位 GUID，移除 "-" 字符。
    /// </summary>
    /// <returns></returns>
    public static string NextId()
    {
        return Guid.NewGuid().ToString("N");
    }
}
