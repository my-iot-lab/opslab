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
                new DeviceVariable("PLC_Sys_Connected", "DB4.0", 0, VariableType.Int,  "", "心跳", VariableFlag.Heartbeat, 500),

                new DeviceVariable("PLC_Sys_E_State", "DB4.2", 0, VariableType.Int,  "", "通知1", VariableFlag.Notice, 10_000),

                new DeviceVariable("PLC_Sys_Sign_Archive_Exe", "DB4.4", 0, VariableType.Int,  "", "Archive", VariableFlag.Trigger, 500)
                {
                     NormalVariables = new()
                     {
                         new DeviceVariable("PLC_Archive_SN", "DB4.30", 20, VariableType.String,  "", "SN", VariableFlag.Normal),
                         new DeviceVariable("PLC_Archive_Cycletime", "DB4.76", 0, VariableType.Int,  "", "CT", VariableFlag.Normal),
                         new DeviceVariable("PLC_Archive_Pass", "DB4.78", 0, VariableType.Int,  "", "Pass", VariableFlag.Normal),
                         new DeviceVariable("PLC_Archive_Operator", "DB4.104", 20, VariableType.String,  "", "Operator", VariableFlag.Normal),
                         new DeviceVariable("PLC_Archive_Voltage", "DB4.126", 20, VariableType.Real,  "", "Voltages", VariableFlag.Normal),
                     },
                },

                new DeviceVariable("MES_ProdTask_Productcode", "DB3.40", 10, VariableType.String,  "", "Productcode", VariableFlag.Normal),
                new DeviceVariable("MES_ProdTask_Amount", "DB3.52", 0, VariableType.Int,  "", "Amount", VariableFlag.Normal),
                new DeviceVariable("MES_ProdTask_Prior", "DB3.54", 0, VariableType.Int,  "", "Prior", VariableFlag.Normal),
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
