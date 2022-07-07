using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ops.Engine.App.ViewModel.Api;
using Ops.Engine.App.ViewModel.Api.WorkVMs;
using WalkingTec.Mvvm.Core;
using WalkingTec.Mvvm.Mvc;

namespace Ops.Engine.App.Api;

/// <summary>
/// 工作
/// </summary>
[ApiController]
[Route("api/[controller]")]
[AllRights]
public class WorkController : BaseApiController
{
    private readonly ILogger _logger;

    public WorkController(ILogger<WorkController> logger)
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
        var vm = Wtm.CreateVM<OutboundWM>();
        var result = vm.Out(data);

        return Ok(result.GetJson());
    }
}
