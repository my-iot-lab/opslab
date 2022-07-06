using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Ops.Engine.App.ViewModel.Api;
using Ops.Engine.App.ViewModel.Api.NotifierVMs;
using WalkingTec.Mvvm.Core;
using WalkingTec.Mvvm.Mvc;

namespace Ops.Engine.App.Api;

[ApiController]
[Route("api/[controller]")]
[AllRights]
public class NotifierController : BaseApiController
{
    private readonly ILogger _logger;

    public NotifierController(ILogger<NotifierController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 通知
    /// </summary>
    [AllowAnonymous]
    [HttpPost("[action]")]
    public IActionResult Notice(ApiData data)
    {
        var vm = Wtm.CreateVM<NotifierVM>();
        var result = vm.Notice(data);

        return Ok(result.GetJson());
    }
}
