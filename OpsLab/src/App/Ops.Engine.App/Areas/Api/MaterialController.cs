using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ops.Engine.App.ViewModel.Api;
using Ops.Engine.App.ViewModel.Api.MaterialVMs;
using WalkingTec.Mvvm.Core;
using WalkingTec.Mvvm.Mvc;

namespace Ops.Engine.App.Areas.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [AllRights]
    public class MaterialController : BaseApiController
    {
        /// <summary>
        /// 扫关键物料
        /// </summary>
        [AllowAnonymous]
        [HttpPost("[action]")]
        public IActionResult Critical(ApiData data)
        {
            var vm = Wtm.CreateVM<MaterialVM>();
            var result = vm.ScanCritical(data);
            
            return Ok(result.GetJson());
        }

        /// <summary>
        /// 扫批次料
        /// </summary>
        [AllowAnonymous]
        [HttpPost("[action]")]
        public IActionResult Batch(ApiData data)
        {
            var vm = Wtm.CreateVM<MaterialVM>();
            var result = vm.ScanBatch(data);

            return Ok(result.GetJson());
        }
    }
}
