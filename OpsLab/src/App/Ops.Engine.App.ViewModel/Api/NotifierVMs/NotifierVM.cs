using WalkingTec.Mvvm.Core;

namespace Ops.Engine.App.ViewModel.Api.NotifierVMs;

public sealed class NotifierVM : BaseVM
{
    /// <summary>
    /// 通知
    /// </summary>
    /// <returns></returns>
    public ApiResult Notice(ApiData data)
    {
        return ApiResult.CreateOK();
    }
}
