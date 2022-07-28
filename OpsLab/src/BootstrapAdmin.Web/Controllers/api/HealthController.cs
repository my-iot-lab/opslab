using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BootstrapAdmin.Web.Controllers;

/// <summary>
/// 健康检查
/// </summary>
[Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
[AllowAnonymous]
[ApiController]
public class HealthController : Controller
{
    [HttpGet("")]
    public IActionResult Index()
    {
        return Ok();
    }
}

