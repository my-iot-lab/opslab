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
[AllRights]
public class TestController : BaseApiController
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
    public IActionResult Test2()
    {
        return JsonMore(new { Code = 1, Code2 = 2, Messsage = "error" });
    }
}

//[ApiController]
[Route("api/[controller]")]
public class TestRawController : Controller
{
    [AllowAnonymous]
    [HttpPost("[action]")]
    public IActionResult Test1()
    {
        var obj = new { Code = 1, Code2 = 2, Messsage = "error" };
        var json = System.Text.Json.JsonSerializer.Serialize(obj);
        return Ok(new { Data = obj, Json = json });
    }

    [AllowAnonymous]
    [HttpPost("[action]")]
    public IActionResult Test2()
    {
        var obj = new { Code = 1, Code2 = 2, Messsage = "error" };
        var json = System.Text.Json.JsonSerializer.Serialize(obj);
        return Content(json);
    }

    [AllowAnonymous]
    [HttpPost("[action]")]
    public IActionResult Test3()
    {
        return Json(new { Code = 1, Code2 = 2, Messsage = "error" });
    }

    [AllowAnonymous]
    [HttpPost("[action]")]
    public IActionResult Test4()
    {
        var obj = new { Code = 1, Code2 = 2, Messsage = "error" };
        var json = System.Text.Json.JsonSerializer.Serialize(obj);
        return Ok(json);
    }
}
