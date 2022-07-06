using WalkingTec.Mvvm.Core;

namespace Ops.Engine.App.ViewModel.Api.WorkVMs;

/// <summary>
/// 出站
/// </summary>
public sealed class OutboundWM : BaseVM
{
    /// <summary>
    /// 出站
    /// </summary>
    /// <returns></returns>
    public ApiResult<ApiData> Out(ApiData data)
    {
        return ApiResult<ApiData>.CreateOK(new ApiData());
    }
}
