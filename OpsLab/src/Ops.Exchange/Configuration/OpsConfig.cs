namespace Ops.Exchange.Configuration;

/// <summary>
/// 配置文件
/// </summary>
public class OpsConfig
{
    /// <summary>
    /// 设备数据存储文件夹
    /// </summary>
    public string DeviceDir { get; set; } = "devices";

    /// <summary>
    /// 轮询监控参数
    /// </summary>
    public MonitorOptions Monitor { get; set; } = new();
}
