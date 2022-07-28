using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Extensions.Options;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Ops.Engine.UI.Config;
using Ops.Engine.UI.Infrastructure;
using Ops.Engine.UI.Views;

namespace Ops.Engine.UI.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly ICollectionView _menuItemsView;
    private ListItem? _selectedItem;
    private int _selectedIndex;
    private string? _searchKeyword;
    private bool _controlsEnabled = true;

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly OpsUIOptions _opsUIOption;

    /// <summary>
    /// 定时器处理对象
    /// </summary>
    public EventHandler? TimerHandler { get; set; }

    public MainViewModel(IHttpClientFactory httpClientFactory, IOptions<OpsUIOptions> opsUIOption)
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

        TimerHandler += (sender, e) =>
        {
            CheckServerHealth();
        };
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

    private string _connHealth = "已断开";
    public string ConnHealth
    {
        get => _connHealth;
        set => SetProperty(ref _connHealth, value);
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

    private void CheckServerHealth()
    {
        var httpClient = _httpClientFactory.CreateClient();
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var response = httpClient.Send(new HttpRequestMessage
            {
                RequestUri = new($"{_opsUIOption.Api.BaseAddress}/api/health"),
                Method = HttpMethod.Get,
            });
            stopwatch.Stop();

            ConnDelay = stopwatch.ElapsedMilliseconds;
            if (response.IsSuccessStatusCode && ConnHealth != "已连接")
            {
                ConnHealth = "已连接";
            }
        }
        catch
        {
            if (ConnHealth != "已断开")
            {
                ConnHealth = "已断开";
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
}
