using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Ops.Host.App.Models;
using Ops.Host.App.UserControls;

namespace Ops.Host.App.ViewModels;

public sealed class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel()
    {
        MenuItemList = GetMenuItems();
    }

    #region 属性绑定

    private ObservableCollection<MenuItemModel>? _menuItemList;
    public ObservableCollection<MenuItemModel>? MenuItemList
    {
        get => _menuItemList;
        set => SetProperty(ref _menuItemList, value);
    }

    private int _selectedIndex = 0;
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

            _selectedItem.Content ??= CreatePage(_selectedItem.ContentType!);
            SubContent = _selectedItem?.Content;
        }
    }

    private object? _subContent = CreatePage(typeof(Home));
    public object? SubContent
    {
        get => _subContent;
        set => SetProperty(ref _subContent, value);
    }

    #endregion

    #region privates

    private ObservableCollection<MenuItemModel> GetMenuItems()
    {
        return new ObservableCollection<MenuItemModel>(new[]
        {
            new MenuItemModel("", "首页", typeof(Home)),
            new MenuItemModel("", "Demo1", typeof(Demo1)),
            new MenuItemModel("", "首页3", null),
        });
    }

    private static object? CreatePage(Type contentType)
    {
        if (typeof(ContentControl).IsAssignableFrom(contentType))
        {
            return Activator.CreateInstance(contentType);
        }

        return default;
    }

    #endregion
}

