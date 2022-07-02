using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using WalkingTec.Mvvm.Core;
using WalkingTec.Mvvm.Core.Extensions;
using WalkingTec.Mvvm.Mvc;
using WalkingTec.Mvvm.Mvc.Admin.ViewModels.ActionLogVMs;

namespace WalkingTec.Mvvm.Admin.Api;

/// <summary>
/// [WTM默认页面 Wtm buidin page]
/// </summary>
[AuthorizeJwtWithCookie]
[ActionDescription("MenuKey.ActionLog")]
[ApiController]
[Route("api/_[controller]")]
public class ActionLogController : BaseApiController
{
    [ActionDescription("Sys.Search")]
    [HttpPost("[action]")]
    public IActionResult Search(ActionLogSearcher searcher)
    {
        if (ModelState.IsValid)
        {
            var vm = Wtm.CreateVM<ActionLogListVM>(passInit: true);
            vm.Searcher = searcher;
            return Content(vm.GetJson());
        }

        return BadRequest(ModelState.GetErrorJson());
    }

    [ActionDescription("Sys.Get")]
    [HttpGet("{id}")]
    public ActionLogVM Get(Guid id)
    {
        var vm = Wtm.CreateVM<ActionLogVM>(id);
        return vm;
    }

    [HttpPost("[action]")]
    [ActionDescription("Sys.Delete")]
    public IActionResult BatchDelete(string[] ids)
    {
        if (ids == null || ids.Length == 0)
        {
            return Ok();
        }

        var vm = Wtm.CreateVM<ActionLogBatchVM>();
        vm.Ids = ids;

        if (!ModelState.IsValid || !vm.DoBatchDelete())
        {
            return BadRequest(ModelState.GetErrorJson());
        }
        return Ok(ids.Length);
    }

    [ActionDescription("Sys.Export")]
    [HttpPost("[action]")]
    public IActionResult ExportExcel(ActionLogSearcher searcher)
    {
        var vm = Wtm.CreateVM<ActionLogListVM>();
        vm.Searcher = searcher;
        vm.SearcherMode = ListVMSearchModeEnum.Export;
        return vm.GetExportData();
    }

    [ActionDescription("Sys.ExportByIds")]
    [HttpPost("[action]")]
    public IActionResult ExportExcelByIds(string[] ids)
    {
        var vm = Wtm.CreateVM<ActionLogListVM>();
        if (ids != null && ids.Length > 0)
        {
            vm.Ids = new List<string>(ids);
            vm.SearcherMode = ListVMSearchModeEnum.CheckExport;
        }
        return vm.GetExportData();
    }
}
