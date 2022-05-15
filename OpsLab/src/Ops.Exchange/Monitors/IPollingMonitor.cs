namespace Ops.Exchange.Monitors;

/// <summary>
/// 轮询监视器
/// </summary>
internal interface IPollingMonitor : IDisposable
{
    /// <summary>
    /// 轮询监听事件
    /// </summary>
    Task PollAsync();

    /// <summary>
    /// 启动监听任务
    /// </summary>
    void Play();

    /// <summary>
    /// 暂停监听任务
    /// </summary>
    void Pasue();
}
