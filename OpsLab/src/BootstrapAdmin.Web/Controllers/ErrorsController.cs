using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RouteAttribute = Microsoft.AspNetCore.Mvc.RouteAttribute;

namespace BootstrapAdmin.Web.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
public class ErrorsController : ControllerBase
{
    private readonly ILogger _logger;

    public ErrorsController(ILogger<ErrorsController> logger)
    {
        _logger = logger;
    }

    [Route("/error")]
    public IActionResult HandleError()
    {
        var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        if (exceptionHandlerPathFeature != null)
        {
            _logger.LogError(exceptionHandlerPathFeature.Error, exceptionHandlerPathFeature.Path);
        }

        return StatusCode(StatusCodes.Status500InternalServerError);
    }
}

