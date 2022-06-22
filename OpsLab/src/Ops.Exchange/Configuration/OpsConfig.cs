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
    /// 轮询监听时间间隔，单位毫秒。考虑每站轮询时间各有差异。
    /// </summary>
    public int PollingInterval { get; set; }
}
