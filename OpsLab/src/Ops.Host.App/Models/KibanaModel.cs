using CommunityToolkit.Mvvm.ComponentModel;

namespace Ops.Host.App.Models;

public sealed class KibanaModel : ObservableObject
{
    /// <summary>
    /// 工站
    /// </summary>
    [NotNull]
    public string? Station { get; set; }

    /// <summary>
    /// 产线
    /// </summary>
    [NotNull]
    public string? Line { get; set; }

    private bool _connectedState;
    /// <summary>
    /// 设备连接状态
    /// </summary>
    public bool ConnectedState
    {
        get => _connectedState;
        set { SetProperty(ref _connectedState, value); }
    }
}
