using WalkingTec.Mvvm.Core;

namespace Ops.Engine.App.ViewModel.Api.MaterialVMs;

public sealed class MaterialVM : BaseVM
{
    /// <summary>
    /// 关键物料
    /// </summary>
    /// <returns></returns>
    public ApiResult ScanCritical(ApiData data)
    {
        return ApiResult.CreateOK();
    }

    /// <summary>
    /// 批次码
    /// </summary>
    /// <returns></returns>
    public ApiResult ScanBatch(ApiData data)
    {
        return ApiResult.CreateOK();
    }
}
