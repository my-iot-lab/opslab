using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
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

    public MainWindowViewModel(
        MonitorManager monitorManager, 
        IOptions<OpsHostOptions> opsHostOption)
    {
        _monitorManager = monitorManager;
        _opsHostOption = opsHostOption.Value;

        MenuItemList = GetMenuItems();
        SelectedItem = MenuItemList.FirstOrDefault(s => s.IsHome); // 设置首页

        RunCommand = new RelayCommand(async () =>
        {
            await RunAsync();
        });
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
            SetProperty(ref _selectedItem, value);

            _selectedItem!.Content ??= CreatePage(_selectedItem.ContentType!);
            SubContent = _selectedItem?.Content;
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
            await _monitorManager.StartAsync();
            Growl.Info("数据监控已启动");
            return;
        }

        Growl.Info("数据监控已关闭");
        _monitorManager.Stop();
    }

    #endregion
}

