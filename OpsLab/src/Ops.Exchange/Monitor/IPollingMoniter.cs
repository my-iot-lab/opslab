namespace Ops.Exchange.Monitor;

/// <summary>
/// 轮询监视器
/// </summary>
internal interface IPollingMoniter
{
    /// <summary>
    /// 轮询
    /// </summary>
    void Poll();
}
