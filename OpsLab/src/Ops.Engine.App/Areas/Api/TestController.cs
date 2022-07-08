using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using WalkingTec.Mvvm.Core;
using WalkingTec.Mvvm.Mvc;

namespace Ops.Engine.App.Areas.Api;

/// <summary>
/// 仅测试使用
/// </summary>
[ApiController]
[Route("api/[controller]")]
//[AllRights]
public class TestController : Controller
{
    private readonly IWebHostEnvironment _env;

    public TestController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [AllowAnonymous]
    [HttpGet("[action]")]
    public IActionResult Env()
    {
        return Ok(_env.EnvironmentName);
    }

    [AllowAnonymous]
    [HttpPost("[action]")]
    public IActionResult Test1()
    {
        return Ok(new { Code = 1, Code2 = 2, Messsage = "error" });
    }

    [AllowAnonymous]
    [HttpPost("[action]")]
    public IActionResult Test2(ApiTestData data)
    {
        return Ok(data);
    }
}

public sealed class ApiTestData
{
    /// <summary>
    /// 请求的 Id，可用于追踪数据。
    /// </summary>
    public string RequestId { get; set; }

    /// <summary>
    /// 事件标签 Tag（唯一）
    /// </summary>
    public string Tag { get; set; }
}
