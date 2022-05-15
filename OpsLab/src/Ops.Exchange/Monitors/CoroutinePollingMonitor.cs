namespace Ops.Exchange.Monitors;

/// <summary>
/// 基于协程的轮询监视器
/// </summary>
internal sealed class CoroutinePollingMonitor : IPollingMonitor
{
    private readonly MonitorOptions _options;
    private readonly CancellationTokenSource _cts = new();

    private int _state; // 1-->运行；2--> 暂停；3-->停止

    // 事件触发器(监控数据变化) <--> 数据派发器(Dispatcher) <--> 任务处理器(Event)
    public CoroutinePollingMonitor(MonitorOptions options)
    {
        _options = options;
    }

    public Task PollAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            if (_state == 1)
            {

            }
        }
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _state = 3;
        _cts.Cancel();
        _cts.Dispose();
    }

    public void Play()
    {
        if (_state == 3)
        {
            return;
        }

        if (_state == 2)
        {
            _state = 1;
        }
    }

    public void Pasue()
    {
        if (_state == 3)
        {
            return;
        }

        if (_state == 1)
        {
            _state = 2;
        }
    }
}
