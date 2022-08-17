namespace Ops.Host.App.Config;

public sealed class OpsHostOptions
{
    /// <summary>
    /// 软件标题名称
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 是否程序启动后自动开始运行。
    /// </summary>
    public bool AutoRunning { get; set; }
}
