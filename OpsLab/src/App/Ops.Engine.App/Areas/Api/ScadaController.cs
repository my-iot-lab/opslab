using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ops.Engine.App.ViewModel.Api;
using Ops.Engine.App.ViewModel.Api.MaterialVMs;
using Ops.Engine.App.ViewModel.Api.NotifierVMs;
using Ops.Engine.App.ViewModel.Api.WorkVMs;
using Ops.Engine.App.ViewModel.Utils;
using System.Text.Json;
using WalkingTec.Mvvm.Core;
using WalkingTec.Mvvm.Mvc;

namespace Ops.Engine.App.Api;

[ApiController]
[Route("api/[controller]")]
[AllRights]
public class ScadaController : BaseApiController
{
    private readonly ILogger _logger;

    public ScadaController(ILogger<ScadaController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 进站
    /// </summary>
    [AllowAnonymous]
    [HttpPost("[action]")]
    public IActionResult Inbound(ApiData data)
    {
        ReBuild(data);

        var wm = Wtm.CreateVM<InboundWM>();
        var result = wm.In(data);

        return Ok(result.GetJson());
    }

    /// <summary>
    /// 出站
    /// </summary>
    [AllowAnonymous]
    [HttpPost("[action]")]
    public IActionResult Outbound(ApiData data)
    {
        ReBuild(data);

        var vm = Wtm.CreateVM<OutboundWM>();
        var result = vm.Out(data);

        return Ok(result.GetJson());
    }

    /// <summary>
    /// 扫关键物料
    /// </summary>
    [AllowAnonymous]
    [HttpPost("[action]")]
    public IActionResult MaterialCritical(ApiData data)
    {
        ReBuild(data);

        var vm = Wtm.CreateVM<MaterialVM>();
        var result = vm.ScanCritical(data);

        return Ok(result.GetJson());
    }

    /// <summary>
    /// 扫批次料
    /// </summary>
    [AllowAnonymous]
    [HttpPost("[action]")]
    public IActionResult MaterialBatch(ApiData data)
    {
        ReBuild(data);

        var vm = Wtm.CreateVM<MaterialVM>();
        var result = vm.ScanBatch(data);

        return Ok(result.GetJson());
    }

    /// <summary>
    /// 通知
    /// </summary>
    [AllowAnonymous]
    [HttpPost("[action]")]
    public IActionResult Notice(ApiData data)
    {
        ReBuild(data);

        var vm = Wtm.CreateVM<NotifierVM>();
        var result = vm.Notice(data);

        return Ok(result.GetJson());
    }

    #region privates

    private static void ReBuild(ApiData data)
    {
        for (int i = 0; i < data.Values.Length; i++)
        {
            var value0 = data.Values[i];
            JsonElement data0 = (JsonElement)value0.Value;
            // 这里只需要区分 bool、int、double 和 string 类型，因为存储只需用这四种类型即可。
            value0.Value = value0.VarType switch
            {
                VariableType.Bit => value0.Length > 0 ? Object2ValueHelper.ToArray<bool>(data0) : Object2ValueHelper.To<bool>(data0),
                VariableType.Byte
                    or VariableType.Word
                    or VariableType.DWord
                    or VariableType.Int
                    or VariableType.DInt => value0.Length > 0 ? Object2ValueHelper.ToArray<int>(data0) : Object2ValueHelper.To<int>(data0),
                VariableType.Real
                    or VariableType.LReal => value0.Length > 0 ? Object2ValueHelper.ToArray<double>(data0) : Object2ValueHelper.To<double>(data0),
                VariableType.String
                    or VariableType.S7String
                    or VariableType.S7WString => Object2ValueHelper.To<string>(data0),
                _ => throw new System.NotImplementedException(),
            };
        }
    }

    #endregion
}
