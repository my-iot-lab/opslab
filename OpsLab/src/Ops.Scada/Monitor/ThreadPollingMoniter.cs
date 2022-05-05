namespace Ops.Scada.Monitor;

/// <summary>
/// 基于线程的轮询监视器
/// </summary>
internal class ThreadPollingMoniter
{
    public void Create()
    {
        _ = new Thread(Execute);
    }

    private void Execute(object obj)
    {

    }
}
