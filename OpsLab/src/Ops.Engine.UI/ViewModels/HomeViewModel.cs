using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Ops.Exchange.Management;
using Ops.Exchange.Monitors;

namespace Ops.Engine.UI.ViewModels;

internal sealed class HomeViewModel : ObservableObject
{
    private Channel<string> _msgChannal = Channel.CreateBounded<string>(32);

    private readonly DeviceInfoManager _deviceInfoManager;
    private readonly MonitorManager _monitorManager;

    public HomeViewModel(DeviceInfoManager deviceInfoManager, MonitorManager monitorManager)
    {
        _deviceInfoManager = deviceInfoManager;
        _monitorManager = monitorManager;

        StartCommand = new RelayCommand(async () =>
        {
            await Start();
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

        _ = Task.Factory.StartNew(() => Thread.Sleep(100)).ContinueWith(async t =>
        {
            while (await _msgChannal.Reader.WaitToReadAsync())
            {
                var msg = await _msgChannal.Reader.ReadAsync();

                if (MessageLogs.Count == 512)
                {
                    MessageLogs.Clear();
                }

                MessageLogs.Add(msg);
                MessageAutoScrollDelegate?.Invoke();
            }
        }, TaskScheduler.FromCurrentSynchronizationContext());
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
    private async Task Start()
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

        var startOptions = new MonitorStartOptions
        {
            NoticeDelegate = (e) =>
            {
                _msgChannal.Writer.TryWrite($"{e.EventTime:yyyy-MM-dd HH:mm:ss:fff}\t{e.Schema.Station} {e.Tag}");
            },
            TriggerDelegate = (e) =>
            {
                _msgChannal.Writer.TryWrite($"{e.EventTime:yyyy-MM-dd HH:mm:ss:fff}\t{e.Context.Request.DeviceInfo.Schema.Station} {e.Tag}");
            },
        };

        await _monitorManager.StartAsync(startOptions);
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

    #endregion
}

public sealed class DeviceState
{
    public string Line { get; set; }

    public string Station { get; set; }

    public bool ConnState { get; set; }
}