using Ops.Exchange.Management;
using Ops.Exchange.Model;
using Ops.Exchange.Monitors;

namespace Ops.Test.ConsoleApp.Suits;

public class MelsecMCSuit : IDisposable
{
    private readonly DeviceInfoManager _deviceInfoManager;
    private readonly MonitorManager _monitorManager;

    public MelsecMCSuit(DeviceInfoManager deviceInfoManager, MonitorManager monitorManager)
    {
        _deviceInfoManager = deviceInfoManager;
        _monitorManager = monitorManager;
    }

    public async Task InitAsync()
    {
        var deviceInfo = new DeviceInfo("L1_OP001", new DeviceSchema("L1", "L1控制线", "OP001", "装配站01", "192.168.3.39", 4096, DriverModel.Melsec_MC));

        List<DeviceVariable> variables = new()
            {
                new DeviceVariable("PLC_Sys_Connected", "D100", 0, VariableType.Int,  "心跳", "", VariableFlag.Heartbeat, 500),

                // 开关信号，拉铆曲线数据
                new DeviceVariable("PLC_Sys_Switch_PullRiveting", "D1000", 0, VariableType.Int,  "拉铆曲线", "", VariableFlag.Switch, 200)
                {
                     NormalVariables = new()
                     {
                         new DeviceVariable("PLC_Switch_PullRiveting_PullingForce", "D1010", 0, VariableType.Real,  "拉力", "", VariableFlag.Normal),
                         new DeviceVariable("PLC_Switch_PullRiveting_Displacement", "D1020", 0, VariableType.Int,  "位移", "", VariableFlag.Normal),
                     },
                },

                // 开关信号，拧紧曲线数据
                new DeviceVariable("PLC_Sys_Switch_ScrewUp", "D1100", 0, VariableType.Int,  "拉铆曲线", "", VariableFlag.Switch, 200)
                {
                     NormalVariables = new()
                     {
                         new DeviceVariable("PLC_Switch_ScrewUp_Angle ", "D1110", 0, VariableType.Real,  "角度", "", VariableFlag.Normal),
                         new DeviceVariable("PLC_Switch_ScrewUp_Torsion ", "D1120", 0, VariableType.Real,  "扭矩", "", VariableFlag.Normal),
                     },
                },
            };

        deviceInfo.AddVariables(variables);

        await _deviceInfoManager.AddAsync(deviceInfo);
    }

    public async Task RunAsync()
    {
        await _monitorManager.StartAsync(new MonitorStartOptions { SwitchPollingInterval = 10 });
    }

    public void Dispose()
    {
        _monitorManager.Dispose();
    }
}
