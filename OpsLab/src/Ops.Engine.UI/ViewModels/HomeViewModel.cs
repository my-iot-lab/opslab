using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Ops.Exchange.Management;

namespace Ops.Engine.UI.ViewModels;

internal sealed class HomeViewModel : ObservableObject
{
    private readonly DeviceInfoManager _deviceInfoManager;

    public HomeViewModel(DeviceInfoManager deviceInfoManager)
    {
        _deviceInfoManager = deviceInfoManager;

        StartCommand = new RelayCommand(() =>
        {
            Start();
        });

        StopCommand = new RelayCommand(() =>
        {
            Stop();
        });

        InitAsync().ConfigureAwait(false).GetAwaiter().GetResult();
    }

    async Task InitAsync()
    {
        var deviceInfos = await _deviceInfoManager.GetAllAsync();
        foreach (var deviceInfo in deviceInfos)
        {
            Devices.Add(new DeviceState
            {
                Line = deviceInfo.Schema.Line,
                Station = deviceInfo.Schema.Station,
                ConnState = false,
            });
        }
    }

    #region 绑定属性

    private ObservableCollection<DeviceState> _devices = new();

    public ObservableCollection<DeviceState> Devices
    {
        get => _devices;
        set { SetProperty(ref _devices, value); }
    }

    private ObservableCollection<LogModel> _messageLogs = new();

    public ObservableCollection<LogModel> MessageLogs
    {
        get => _messageLogs;
        set { SetProperty(ref _messageLogs, value); }
    }

    #endregion

    #region Commands

    public ICommand StartCommand { get; }

    public ICommand StopCommand { get; }

    #endregion

    #region Privates

    private void Start()
    {

    }

    private void Stop()
    {

    }

    #endregion
}

public sealed class DeviceState
{
    public string Line { get; set; }

    public string Station { get; set; }

    public bool ConnState { get; set; }
}

public sealed class LogModel
{
    public string Time { get; set; }

    public string Message { get; set; }
}