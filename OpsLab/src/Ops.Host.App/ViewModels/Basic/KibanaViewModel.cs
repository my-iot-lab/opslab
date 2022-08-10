using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Ops.Exchange.Management;
using Ops.Host.App.Models;

namespace Ops.Host.App.ViewModels;

public sealed class KibanaViewModel : ObservableObject
{
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

        // 状态检测
        _deviceHealthManager.Check();
        TimerHandler += (sender, e) =>
        {
            ChangeDeviceConnState();
        };
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
            device.ConnectedState = _deviceHealthManager.CanConnect(device.Line, device.Station);
        }
    }

    #endregion
}
