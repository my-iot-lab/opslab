﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Extensions.Options;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Ops.Engine.Scada.Config;
using Ops.Engine.Scada.Infrastructure;
using Ops.Engine.Scada.Views;

namespace Ops.Engine.Scada.ViewModels;

public class MainViewModel : ObservableObject, IDisposable
{
    private readonly CancellationTokenSource _cts = new();

    private readonly ICollectionView _menuItemsView;
    private ListItem? _selectedItem;
    private int _selectedIndex;
    private string? _searchKeyword;
    private bool _controlsEnabled = true;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OpsScadaOptions _opsUIOption;

    public MainViewModel(IHttpClientFactory httpClientFactory, IOptions<OpsScadaOptions> opsUIOption)
    {
        _httpClientFactory = httpClientFactory;
        _opsUIOption = opsUIOption.Value;

        MenuItems = GetMenuItems();
        _menuItemsView = CollectionViewSource.GetDefaultView(MenuItems);
        _menuItemsView.Filter = MenuFilter;

        HomeCommand = new RelayCommand(() =>
        {
            SearchKeyword = string.Empty;
            SelectedIndex = 0;
        });

        MovePrevCommand = new RelayCommand(() =>
        {
            if (!string.IsNullOrWhiteSpace(SearchKeyword))
                SearchKeyword = string.Empty;

            SelectedIndex--;
        }, () => SelectedIndex > 0);

        MoveNextCommand = new RelayCommand(() =>
        {
            if (!string.IsNullOrWhiteSpace(SearchKeyword))
                SearchKeyword = string.Empty;

            SelectedIndex++;
        }, () => SelectedIndex < MenuItems.Count - 1);

        // 此处不采用 DispatcherTimer 定时器，因为调用若方法耗时长且为同步执行，会导致 UI 卡住。
        _ = Task.Factory.StartNew(async () =>
        {
            //var timer = new System.Threading.PeriodicTimer(TimeSpan.FromSeconds(2));
            while (!_cts.IsCancellationRequested)
            {
                await Task.Delay(2000);

                await CheckServerHealthAsync();
            }
        }, default, default, TaskScheduler.FromCurrentSynchronizationContext());
    }

    #region 绑定属性

    public string Title => _opsUIOption.Title ?? "SCADA";

    public ObservableCollection<ListItem> MenuItems { get; }

    public string? SearchKeyword
    {
        get => _searchKeyword;
        set
        {
            if (SetProperty(ref _searchKeyword, value))
            {
                _menuItemsView.Refresh();
            }
        }
    }

    public ListItem? SelectedItem
    {
        get => _selectedItem;
        set => SetProperty(ref _selectedItem, value);
    }

    public int SelectedIndex
    {
        get => _selectedIndex;
        set => SetProperty(ref _selectedIndex, value);
    }

    public bool ControlsEnabled
    {
        get => _controlsEnabled;
        set => SetProperty(ref _controlsEnabled, value);
    }

    private string _appVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";
    public string AppVersion
    {
        get => _appVersion;
        set => SetProperty(ref _appVersion, value);
    }

    private bool _connState;
    public bool ConnState
    {
        get => _connState;
        set => SetProperty(ref _connState, value);
    }

    private long _connDelay;
    public long ConnDelay
    {
        get => _connDelay;
        set => SetProperty(ref _connDelay, value);
    }

    #endregion

    #region 绑定事件

    public ICommand HomeCommand { get; }

    public ICommand MovePrevCommand { get; }

    public ICommand MoveNextCommand { get; }

    #endregion

    #region privates 

    private async Task CheckServerHealthAsync()
    {
        var httpClient = _httpClientFactory.CreateClient();
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = await httpClient.GetAsync(new Uri($"{_opsUIOption.Api.BaseAddress}/api/health"));
            stopwatch.Stop();

            ConnDelay = stopwatch.ElapsedMilliseconds;
            if (response.IsSuccessStatusCode && !ConnState)
            {
                ConnState = true;
            }
        }
        catch
        {
            if (ConnState)
            {
                ConnState = false;
            }
        }
    }

    private bool MenuFilter(object obj)
    {
        if (string.IsNullOrWhiteSpace(_searchKeyword))
        {
            return true;
        }

        return obj is ListItem item && item.Name.ToLower().Contains(_searchKeyword!.ToLower());
    }

    private ObservableCollection<ListItem> GetMenuItems()
    {
        return new ObservableCollection<ListItem>(new[]
        {
            new ListItem("首页", typeof(Home)),
            new ListItem("地址变量", typeof(Address)),
            new ListItem("程序状态", typeof(AppDiagnostic)),
        });
    }

    #endregion

    public void Dispose()
    {
        _cts.Cancel();
    }
}
