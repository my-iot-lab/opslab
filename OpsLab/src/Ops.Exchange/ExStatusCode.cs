namespace Ops.Exchange;

/// <summary>
/// 状态码。
/// <para>0 初始状态/成功处理状态</para>
/// <para>1 数据触发状态</para>
/// <para>2~9 常用内部异常状态</para>
/// <para>100~199 进站状态</para>
/// <para>200~299 出站状态</para>
/// <para>300~399 扫码状态</para>
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
    /// 事件处理异常
    /// </summary>
    public const short HandlerException = 2;

    /// <summary>
    /// 事件处理超时
    /// </summary>
    public const short HandlerTimeout = 3;

    /// <summary>
    /// 远程请求/调用异常
    /// </summary>
    public const short RemoteException = 4;
}
