using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BootstrapAdmin.Web.Models;
using BootstrapAdmin.Web.Utils;

namespace BootstrapAdmin.Web.Controllers;

[AllowAnonymous]
[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
public class ScadaController : Controller
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

        return Ok();
    }

    /// <summary>
    /// 出站
    /// </summary>
    [AllowAnonymous]
    [HttpPost("[action]")]
    public IActionResult Outbound(ApiData data)
    {
        ReBuild(data);

        return Ok();
    }

    /// <summary>
    /// 扫关键物料
    /// </summary>
    [HttpPost("[action]")]
    public IActionResult MaterialCritical(ApiData data)
    {
        ReBuild(data);

        return Ok();
    }

    /// <summary>
    /// 扫批次料
    /// </summary>
    [AllowAnonymous]
    [HttpPost("[action]")]
    public IActionResult MaterialBatch(ApiData data)
    {
        ReBuild(data);

        return Ok();
    }

    /// <summary>
    /// 通知
    /// </summary>
    [AllowAnonymous]
    [HttpPost("[action]")]
    public IActionResult Notice(ApiData data)
    {
        ReBuild(data);

        return Ok();
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
