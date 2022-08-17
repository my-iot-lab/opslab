using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Ops.Exchange.Management;
using Ops.Host.App.Models;

namespace Ops.Host.App.ViewModels;

public sealed class KibanaViewModel : ObservableObject, IDisposable
{
    private readonly CancellationTokenSource _cts = new();

    private readonly DeviceInfoManager _deviceInfoManager;
    private readonly DeviceHealthManager _deviceHealthManager;

    /// <summary>
    /// 定时器处理对象
    /// </summary>
    public EventHandler? TimerHandler { get; set; }

    public KibanaViewModel(
        DeviceInfoManager deviceInfoManager,
        DeviceHealthManager deviceHealthManager)
    {
        _deviceInfoManager = deviceInfoManager;
        _deviceHealthManager = deviceHealthManager;

        Init();
    }

    void Init()
    {
        var deviceInfos = _deviceInfoManager.GetAll();
        foreach (var deviceInfo in deviceInfos)
        {
            DeviceSourceList.Add(new KibanaModel
            {
                Line = deviceInfo.Schema.Line,
                Station = deviceInfo.Schema.Station,
                ConnectedState = false,
            });
        }

        // 状态检测，定时器可考虑与 DispatcherTimer 有什么差异
        _deviceHealthManager.Check();
        _ = Task.Factory.StartNew(async () =>
        {
            while (!_cts.IsCancellationRequested)
            {
                await Task.Delay(2000);
                ChangeDeviceConnState();
            }
                
        }, default, TaskCreationOptions.LongRunning, TaskScheduler.FromCurrentSynchronizationContext());
    }

    #region 绑定属性

    private ObservableCollection<KibanaModel> _deviceSourceList = new();

    public ObservableCollection<KibanaModel> DeviceSourceList
    {
        get => _deviceSourceList;
        set => SetProperty(ref _deviceSourceList, value);
    }

    #endregion

    #region privates

    private void ChangeDeviceConnState()
    {
        foreach (var device in DeviceSourceList)
        {
            var state = _deviceHealthManager.CanConnect(device.Line, device.Station);
            if (device.ConnectedState != state)
            {
                device.ConnectedState = state;
            }
        }
    }

    #endregion

    public void Dispose()
    {
        _cts.Cancel();
    }
}
