using Ops.Exchange.Management;
using Ops.Exchange.Model;
using Ops.Exchange.Monitors;

namespace Ops.Test.ConsoleApp.Suits;

public class SimaticS7Suit : IDisposable
{
    private readonly DeviceInfoManager _deviceInfoManager;
    private readonly MonitorManager _monitorManager;

    public SimaticS7Suit(DeviceInfoManager deviceInfoManager, MonitorManager monitorManager)
    {
        _deviceInfoManager = deviceInfoManager;
        _monitorManager = monitorManager;
    }

    public async Task InitAsync()
    {
        var deviceInfo = new DeviceInfo("L1_OP001", new DeviceSchema("L1", "L1控制线", "OP001", "装配站01", "192.168.0.1", DriverModel.S7_1500));

        List<DeviceVariable> variables = new()
            {
                new DeviceVariable("Heartbeat", "DB4.0", 0, VariableType.Int, "心跳", VariableFlag.Heartbeat, 500),

                //new DeviceVariable("Notice1", "s=1;x=3;10", 0, VariableType.Int, "通知1", VariableFlag.Notice, 2000),
                //new DeviceVariable("Notice2", "s=1;x=3;11", 0, VariableType.Int, "通知2", VariableFlag.Notice, 3000),
                //new DeviceVariable("Notice3", "s=1;x=3;12", 0, VariableType.Int, "通知3", VariableFlag.Notice, 4000),
                //new DeviceVariable("Notice4", "s=1;x=3;13", 0, VariableType.Int, "通知4", VariableFlag.Notice, 5000),

                //new DeviceVariable("Trigger1", "s=1;x=3;20", 0, VariableType.Int, "触发1", VariableFlag.Trigger, 500)
                //{
                //     NormalVariables = new()
                //     {
                //         new DeviceVariable("Trigger1_Data1", "s=1;x=3;21", 0, VariableType.Int, "数据1", VariableFlag.Normal),
                //         new DeviceVariable("Trigger1_Data2", "s=1;x=3;22", 0, VariableType.Int, "数据2", VariableFlag.Normal),
                //         new DeviceVariable("Trigger1_Data3", "s=1;x=3;23", 0, VariableType.Int, "数据3", VariableFlag.Normal),
                //     },
                //},

                //new DeviceVariable("Normal_1", "s=1;x=3;90", 0, VariableType.Int, "普通数据1", VariableFlag.Normal),
                //new DeviceVariable("Normal_2", "s=1;x=3;91", 0, VariableType.Int, "普通数据2", VariableFlag.Normal),
                //new DeviceVariable("Normal_3", "s=1;x=3;92", 0, VariableType.Int, "普通数据3", VariableFlag.Normal),
            };

        deviceInfo.AddVariables(variables);

        await _deviceInfoManager.AddAsync(deviceInfo);
    }

    public async Task RunAsync()
    {
        await _monitorManager.StartAsync();
    }

    public void Dispose()
    {
        _monitorManager.Dispose();
    }
}
