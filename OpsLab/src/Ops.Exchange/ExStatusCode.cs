namespace Ops.Exchange;

/// <summary>
/// 状态码，需注意自定义状态码不能冲突。
/// <para>【0】 初始状态/成功处理状态；</para>
/// <para>【1】 数据触发状态；</para>
/// <para>【2】 内部异常统一状态；</para>
/// <para>【400~499】 请求数据异常，如参数为空、数据类型不对等；</para>
/// <para>【500~599】 内部异常，如超时、崩溃等；</para>
/// <para>【1100~1199】 进站详细状态；</para>
/// <para>【1200~1299】 出站详细状态；</para>
/// <para>【1300~1399】 扫码详细状态；</para>
/// <para>【1400~1499】 其他状态。</para>
/// </summary>
public sealed class ExStatusCode 
{
    /// <summary>
    /// 无状态
    /// </summary>
    public const short None = 0;

    /// <summary>
    /// 事件处理成功
    /// </summary>
    public const short Success = 0;

    /// <summary>
    /// 数据触发状态
    /// </summary>
    public const short Trigger = 1;

    /// <summary>
    /// 开发开启状态
    /// </summary>
    public const short SwitchOn = 1;

    /// <summary>
    /// 事件处理异常
    /// </summary>
    public const short HandlerException = 2;

    #region 【500~599】内部异常

    /// <summary>
    /// 事件处理超时
    /// </summary>
    public const short HandlerTimeout = 501;

    /// <summary>
    /// 远程请求/调用异常
    /// </summary>
    public const short RemoteException = 502;

    #endregion
}
