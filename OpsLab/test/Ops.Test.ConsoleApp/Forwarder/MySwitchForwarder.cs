using Microsoft.Extensions.Logging;
using Ops.Exchange.Forwarder;
using Ops.Exchange.Model;

namespace Ops.Test.ConsoleApp.Forwarder;

internal class MySwitchForwarder : ISwitchForwarder
{
    private readonly ILogger _logger;

    public MySwitchForwarder(ILogger<MySwitchForwarder> logger)
    {
        _logger = logger;
    }

    public Task ExecuteAsync(SwitchForwardData data, CancellationToken cancellationToken = default)
    {
        // 拉铆曲线
        if (data.Data.Tag == "PLC_Sys_Switch_PullRiveting")
        {
            var pullingForce = data.Data.GetReal("PLC_Switch_PullRiveting_PullingForce");
            var displacement = data.Data.GetInt("PLC_Switch_PullRiveting_Displacement");

            switch (data.SwitchState)
            {
                case SwitchState.Ready:
                    _logger.LogInformation($"{data.Data.RequestId} - {data.Data.Schema.Station} - {data.Data.Tag} - 启动信号");
                    break;
                case SwitchState.On:
                    _logger.LogInformation($"{data.Data.RequestId} - {data.Data.Schema.Station} - {data.Data.Tag} - 拉力: {pullingForce}, 位移: {displacement}");
                    break;
                case SwitchState.Off:
                    _logger.LogInformation($"{data.Data.RequestId} - {data.Data.Schema.Station} - {data.Data.Tag} - 关闭信号");
                    break;
                default:
                    break;
            }
        }
        else if (data.Data.Tag == "PLC_Sys_Switch_ScrewUp") // 拧紧曲线
        {
            var angle = data.Data.GetReal("PLC_Switch_ScrewUp_Angle");
            var torsion = data.Data.GetReal("PLC_Switch_ScrewUp_Torsion");

            switch (data.SwitchState)
            {
                case SwitchState.Ready:
                    _logger.LogInformation($"{data.Data.RequestId} - {data.Data.Schema.Station} - {data.Data.Tag} - 启动信号");
                    break;
                case SwitchState.On:
                    _logger.LogInformation($"{data.Data.RequestId} - {data.Data.Schema.Station} - {data.Data.Tag} -  角度: {angle}, 扭矩: {torsion}");
                    break;
                case SwitchState.Off:
                    _logger.LogInformation($"{data.Data.RequestId} - {data.Data.Schema.Station} - {data.Data.Tag} - 关闭信号");
                    break;
                default:
                    break;
            }
        }





        return Task.CompletedTask;
    }
}
