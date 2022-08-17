using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HandyControl.Controls;
using Ops.Exchange.Monitors;
using Ops.Host.App.Config;
using Ops.Host.App.Management;
using Ops.Host.App.Models;

namespace Ops.Host.App.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    private readonly MonitorManager _monitorManager;
    private readonly OpsHostOptions _opsHostOption;
    private readonly ILogger _logger;

    public MainWindowViewModel(
        MonitorManager monitorManager, 
        IOptions<OpsHostOptions> opsHostOption,
        ILogger<MainWindowViewModel> logger)
    {
        _monitorManager = monitorManager;
        _opsHostOption = opsHostOption.Value;
        _logger = logger;

        MenuItemList = GetMenuItems();
        SelectedItem = MenuItemList.FirstOrDefault(s => s.IsHome); // 设置首页

        RunCommand = new RelayCommand(async () =>
        {
            await RunAsync();
        });

        Init();
    }

    void Init()
    {
        // 检测是否为自动运行
        if (_opsHostOption.AutoRunning)
        {
            try
            {
                _isRunning = true;
                RunAsync().RunSynchronously();
            }
            catch
            { }
        }
    }

    #region 属性绑定

    public string Title => _opsHostOption.Title ?? "上位机系统";

    private ObservableCollection<MenuItemModel>? _menuItemList;
    public ObservableCollection<MenuItemModel>? MenuItemList
    {
        get => _menuItemList;
        set => SetProperty(ref _menuItemList, value);
    }

    private int _selectedIndex;
    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetProperty(ref _selectedIndex, value);
    }

    private MenuItemModel? _selectedItem;
    public MenuItemModel? SelectedItem
    {
        get => _selectedItem;
        set 
        {
            if (SetProperty(ref _selectedItem, value))
            {
                if (_selectedItem != null)
                {
                    _selectedItem.Content ??= CreatePage(_selectedItem.ContentType);
                    SubContent = _selectedItem?.Content;
                }
            }
        }
    }

    private object? _subContent;
    public object? SubContent
    {
        get => _subContent;
        set => SetProperty(ref _subContent, value);
    }

    private bool _isRunning = false;
    public bool IsRunning
    {
        get => _isRunning;
        set { SetProperty(ref _isRunning, value); }
    }

    #endregion

    #region 事件绑定

    public ICommand RunCommand { get; }

    #endregion

    #region privates

    private static ObservableCollection<MenuItemModel> GetMenuItems()
    {
        return new ObservableCollection<MenuItemModel>(MenuManager.Menus);
    }

    private static object? CreatePage(Type contentType)
    {
        if (typeof(ContentControl).IsAssignableFrom(contentType))
        {
            return Activator.CreateInstance(contentType);
        }

        return default;
    }

    private async Task RunAsync()
    {
        if (_isRunning)
        {
            try
            {
                await _monitorManager.StartAsync();
                Growl.Info("数据监控已启动");

                IsRunning = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "数据监控启动失败");
                Growl.Error("监控启动失败，请检测能否访问 PLC 地址，然后再重新启动。");

                IsRunning = false;
            }

            return;
        }

        _monitorManager.Stop();
        Growl.Info("数据监控已关闭");
        IsRunning = false;
    }

    #endregion
}

