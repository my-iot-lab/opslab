using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ops.Engine.App.ViewModel.Api;
using Ops.Engine.App.ViewModel.Api.MaterialVMs;
using Ops.Engine.App.ViewModel.Api.NotifierVMs;
using Ops.Engine.App.ViewModel.Api.WorkVMs;
using System.Collections.Generic;
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

    private void ReBuild(ApiData data)
    {
        for (int i = 0; i < data.Values.Length; i++)
        {
            var value0 = data.Values[i];
            JsonElement data0 = (JsonElement)value0.Value;
            // 这里只需要区分 bool、int、double 和 string 类型，因为存储只需用这四种类型即可。
            value0.Value = value0.VarType switch
            {
                VariableType.Bit => value0.Length > 0 ? JsonObject2Array<bool>(data0) : JsonObjectTo<bool>(data0),
                VariableType.Byte
                    or VariableType.Word
                    or VariableType.DWord
                    or VariableType.Int
                    or VariableType.DInt => value0.Length > 0 ? JsonObject2Array<int>(data0) : JsonObjectTo<int>(data0),
                VariableType.Real
                    or VariableType.LReal => value0.Length > 0 ? JsonObject2Array<double>(data0) : JsonObjectTo<double>(data0),
                VariableType.String
                    or VariableType.S7String
                    or VariableType.S7WString => JsonObjectTo<string>(data0),
                _ => throw new System.NotImplementedException(),
            };
        }
    }

    T[] JsonObject2Array<T>(JsonElement jsonElement)
    {
        if (jsonElement.ValueKind == JsonValueKind.Array)
        {
            var len = jsonElement.GetArrayLength();
            var arr = new List<T>(len);
            foreach (var item in jsonElement.EnumerateArray())
            {
                T obj = JsonObjectTo<T>(item);
                arr.Add(obj);
            }

            return arr.ToArray();
        }

        return System.Array.Empty<T>();
    }

    T JsonObjectTo<T>(JsonElement jsonElement)
    {
        object? obj = null;
        if (jsonElement.ValueKind == JsonValueKind.True)
        {
            obj = true;
        }
        else if (jsonElement.ValueKind == JsonValueKind.False)
        {
            obj = false;
        }
        else if (jsonElement.ValueKind == JsonValueKind.Number)
        {
            if (typeof(T) == typeof(byte))
            {
                obj = jsonElement.GetByte();
            }
            else if (typeof(T) == typeof(sbyte))
            {
                obj = jsonElement.GetSByte();
            }
            else if (typeof(T) == typeof(ushort))
            {
                obj = jsonElement.GetUInt16();
            }
            else if (typeof(T) == typeof(short))
            {
                obj = jsonElement.GetInt16();
            }
            else if (typeof(T) == typeof(uint))
            {
                obj = jsonElement.GetUInt32();
            }
            else if (typeof(T) == typeof(int))
            {
                obj = jsonElement.GetInt32();
            }
            else if (typeof(T) == typeof(float))
            {
                obj = jsonElement.GetSingle();
            }
            else if (typeof(T) == typeof(double))
            {
                obj = jsonElement.GetDouble();
            }
        }
        else if (jsonElement.ValueKind == JsonValueKind.String)
        {
            if (typeof(T) == typeof(string))
            {
                obj = jsonElement.GetString();
            }
        }

        return (T)obj;
    }

    #endregion
}
