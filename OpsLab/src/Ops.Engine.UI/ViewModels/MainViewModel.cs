using System.Collections.ObjectModel;
using System.ComponentModel;
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

    private readonly OpsUIOptions _opsUIOption;

    public MainViewModel(IOptions<OpsUIOptions> opsUIOption)
    {
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

    #endregion

    #region 绑定事件

    public ICommand HomeCommand { get; }

    public ICommand MovePrevCommand { get; }

    public ICommand MoveNextCommand { get; }

    #endregion

    #region privates 

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
        });
    }

    #endregion
}
