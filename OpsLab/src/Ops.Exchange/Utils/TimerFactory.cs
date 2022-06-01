namespace Ops.Exchange.Utils;

/// <summary>
/// 定时器工厂
/// </summary>
internal class TimerFactory
{
    /// <summary>
    /// 创建一个新的定时器，该定时器基于线程池。
    /// </summary>
    /// <param name="callback">回调函数</param>
    /// <param name="state">回调函数参数</param>
    /// <param name="dueTime">延迟启动时长（毫秒）</param>
    /// <param name="period">轮询时间间隔（毫秒）</param>
    /// <returns></returns>
    public static ITimer CreateTimer(TimerCallback callback, object state, long dueTime, long period)
    {
        return new TimerWrapper(callback, state, dueTime, period);
    }
}
