using WalkingTec.Mvvm.Core;

namespace Ops.Engine.App.ViewModel.Api.WorkVMs;

/// <summary>
/// 进站
/// </summary>
public sealed class InboundWM : BaseVM
{
    /// <summary>
    /// 进站
    /// </summary>
    /// <returns></returns>
    public ApiResult<ApiData> In(ApiData data)
    {
        return ApiResult<ApiData>.CreateOK(new ApiData());
    }
}
