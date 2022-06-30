using Ops.Exchange.Management;
using Ops.Exchange.Model;
using Ops.Exchange.Monitors;

namespace Ops.Test.ConsoleApp.Suits;

/// <summary>
/// ModbusTcp 测试套装
/// </summary>
public sealed class ModbusTcpSuit : IDisposable
{
    private readonly DeviceInfoManager _deviceInfoManager;
    private readonly MonitorManager _monitorManager;

    public ModbusTcpSuit(DeviceInfoManager deviceInfoManager, MonitorManager monitorManager)
    {
        _deviceInfoManager = deviceInfoManager;
        _monitorManager = monitorManager;
    }

    public async Task InitAsync()
    {
        var deviceInfo = new DeviceInfo(new DeviceSchema
        {
            Id = 1,
            Line = "L1",
            LineName = "L1控制线",
            Station = "OP001",
            StationName = "上线装配站",
            Host = "127.0.0.1",
            DriverModel = DriverModel.ModbusTcp,
        })
        {
            Id = 1
        };

        List<DeviceVariable> variables = new()
        {
            //new DeviceVariable
            //{
            //     Id = 1,
            //     Tag = "Heartbeat",
            //     Address = "s=1;x=3;2", // 设备号1，功能能码03，地址2（基地址为0）
            //     Length = 0,
            //     VarType = VariableType.Int,
            //     Desc = "心跳",
            //     Flag = VariableFlag.Heartbeat,
            //     PollingInterval = 500,
            //},

            //new DeviceVariable
            //{
            //     Id = 2,
            //     Tag = "Notice1",
            //     Address = "s=1;x=3;10",
            //     Length = 0,
            //     VarType = VariableType.Int,
            //     Desc = "通知1",
            //     Flag = VariableFlag.Notice,
            //     PollingInterval = 2000,
            //},
            //new DeviceVariable
            //{
            //     Id = 3,
            //     Tag = "Notice2",
            //     Address = "s=1;x=3;11",
            //     Length = 0,
            //     VarType = VariableType.Int,
            //     Desc = "通知2",
            //     Flag = VariableFlag.Notice,
            //     PollingInterval = 3000,
            //},
            //new DeviceVariable
            //{
            //     Id = 4,
            //     Tag = "Notice3",
            //     Address = "s=1;x=3;12",
            //     Length = 0,
            //     VarType = VariableType.Int,
            //     Desc = "通知3",
            //     Flag = VariableFlag.Notice,
            //     PollingInterval = 4000,
            //},
            //new DeviceVariable
            //{
            //     Id = 5,
            //     Tag = "Notice4",
            //     Address = "s=1;x=3;13",
            //     Length = 0,
            //     VarType = VariableType.Int,
            //     Desc = "通知4",
            //     Flag = VariableFlag.Notice,
            //     PollingInterval = 5000,
            //},

            new DeviceVariable
            {
                 Id = 6,
                 Tag = "Trigger1",
                 Address = "s=1;x=3;20",
                 Length = 0,
                 VarType = VariableType.Int,
                 Desc = "触发1",
                 Flag = VariableFlag.Trigger,
                 PollingInterval = 500,
                 NormalVariables = new()
                 {
                     new DeviceVariable
                     {
                         Id = 61,
                         Tag = "Trigger1_Data1",
                         Address = "s=1;x=3;21",
                         Length = 0,
                         VarType = VariableType.Int,
                         Desc = "数据1",
                         Flag = VariableFlag.Normal,
                     },
                     new DeviceVariable
                     {
                         Id = 62,
                         Tag = "Trigger1_Data2",
                         Address = "s=1;x=3;22",
                         Length = 0,
                         VarType = VariableType.Int,
                         Desc = "数据1",
                         Flag = VariableFlag.Normal,
                     },
                     new DeviceVariable
                     {
                         Id = 63,
                         Tag = "Trigger1_Data3",
                         Address = "s=1;x=3;23",
                         Length = 0,
                         VarType = VariableType.Int,
                         Desc = "数据1",
                         Flag = VariableFlag.Normal,
                     },
                 },
            },
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
