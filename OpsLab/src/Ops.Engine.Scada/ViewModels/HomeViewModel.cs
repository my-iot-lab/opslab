using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Ops.Engine.Scada.Managements;
using Ops.Exchange.Management;
using Ops.Exchange.Monitors;

namespace Ops.Engine.Scada.ViewModels;

internal sealed class HomeViewModel : ObservableObject
{
    private readonly DeviceInfoManager _deviceInfoManager;
    private readonly DeviceHealthManager _deviceHealthManager;
    private readonly MonitorManager _monitorManager;

    /// <summary>
    /// 定时器处理对象
    /// </summary>
    public EventHandler? TimerHandler { get; set; }

    public HomeViewModel(DeviceInfoManager deviceInfoManager,
        DeviceHealthManager deviceHealthManager,
        MonitorManager monitorManager)
    {
        _deviceInfoManager = deviceInfoManager;
        _deviceHealthManager = deviceHealthManager;
        _monitorManager = monitorManager;

        StartCommand = new RelayCommand(async () =>
        {
            await StartAsync();
        });

        StopCommand = new RelayCommand(() =>
        {
            Stop();
        });

        Init();
    }

    void Init()
    {
        var deviceInfos = _deviceInfoManager.GetAll();
        foreach (var deviceInfo in deviceInfos)
        {
            Devices.Add(new DeviceState
            {
                Line = deviceInfo.Schema.Line,
                Station = deviceInfo.Schema.Station,
                ConnState = false,
            });
        }

        _ = Task.Factory.StartNew(async () =>
        {
            var logMsgChannel = GlobalChannelManager.Defalut.LogMessageChannel;
            while (await logMsgChannel.Reader.WaitToReadAsync())
            {
                var msg = await logMsgChannel.Reader.ReadAsync();

                if (MessageLogs.Count == 512)
                {
                    MessageLogs.Clear();
                }

                MessageLogs.Add($"{msg.CreatedTime:yyyy-MM-dd HH:mm:ss:fff}\t{msg.Line}\t{msg.Station}\t{msg.Tag}\t{msg.Message}");
                MessageAutoScrollDelegate?.Invoke();
            }
        }, default, default, TaskScheduler.FromCurrentSynchronizationContext());

        // 状态检测
        _deviceHealthManager.Check();
        TimerHandler += (sender, e) =>
        {
            ChangeDeviceConnState();
        };
    }

    /// <summary>
    /// 消息栏滚动事件
    /// </summary>
    public Action? MessageAutoScrollDelegate { get; set; }

    /// <summary>
    /// 提示消息事件
    /// </summary>
    public Action<string>? MessageTipDelegate { get; set; }

    #region 绑定属性

    private ObservableCollection<DeviceState> _devices = new();

    public ObservableCollection<DeviceState> Devices
    {
        get => _devices;
        set { SetProperty(ref _devices, value); }
    }

    private ObservableCollection<string> _messageLogs = new();

    public ObservableCollection<string> MessageLogs
    {
        get => _messageLogs;
        set { SetProperty(ref _messageLogs, value); }
    }

    private bool _isRunning = false;
    public bool IsRunning
    {
        get => _isRunning;
        set { SetProperty(ref _isRunning, value); }
    }

    private bool _isStop = true;
    public bool IsStop
    {
        get => _isStop;
        set { SetProperty(ref _isStop, value); }
    }

    #endregion

    #region Commands

    public ICommand StartCommand { get; }

    public ICommand StopCommand { get; }

    #endregion

    #region Privates

    /// <summary>
    /// 启动监听
    /// </summary>
    private async Task StartAsync()
    {
        if (Devices.Count == 0)
        {
            MessageTipDelegate?.Invoke("无可监控的设备");
            return;
        }

        if (_isRunning)
        {
            return;
        }
        IsRunning = true;
        IsStop = false;

        await _monitorManager.StartAsync();
    }

    /// <summary>
    /// 停止监听
    /// </summary>
    private void Stop()
    {
        if (!_isRunning)
        {
            return;
        }
        IsRunning = false;
        IsStop = true;

        _monitorManager.Stop();
    }

    private void ChangeDeviceConnState()
    {
        foreach (var device in Devices)
        {
            device.ConnState = _deviceHealthManager.CanConnect(device.Line, device.Station);
        }
    }

    #endregion
}

// 因为要更新 ObservableCollection 集合中元素的属性，因此对应元素也需要实现 INotifyPropertyChanged 接口。
public sealed class DeviceState : ObservableObject
{
    [NotNull]
    public string? Line { get; set; }

    [NotNull]
    public string? Station { get; set; }

    private bool _connState;
    /// <summary>
    /// 设备连接状态
    /// </summary>
    public bool ConnState
    {
        get => _connState;
        set { SetProperty(ref _connState, value); }
    }
}