using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using MaterialDesignThemes.Wpf;
using Ops.Engine.UI.Domain.ViewModels;

namespace Ops.Engine.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContext = App.Current.Services.GetService<MainViewModel>();

        var paletteHelper = new PaletteHelper();
        var theme = paletteHelper.GetTheme();

        DarkModeToggleButton.IsChecked = theme.GetBaseTheme() == BaseTheme.Dark;

        if (paletteHelper.GetThemeManager() is { } themeManager)
        {
            themeManager.ThemeChanged += (_, e)
                => DarkModeToggleButton.IsChecked = e.NewTheme?.GetBaseTheme() == BaseTheme.Dark;
        }
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonDown(e);

        // 重写，当窗体设置 WindowStyle=none 时可移动窗体。
        // 窗体上按下鼠标左键，可拖拽。
        if (WindowStyle == WindowStyle.None)
        {
            this.DragMove();
        }
    }

    private void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        //until we had a StaysOpen glag to Drawer, this will help with scroll bars
        var dependencyObject = Mouse.Captured as DependencyObject;

        while (dependencyObject != null)
        {
            if (dependencyObject is ScrollBar)
                return;

            dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
        }

        MenuToggleButton.IsChecked = false;
    }

    private void MaximizedButton_OnClick(object sender, RoutedEventArgs e)
    {
        if (WindowStyle == WindowStyle.None)
        {
            WindowState = WindowState.Normal;
            WindowStyle = WindowStyle.SingleBorderWindow;
        }
        else if (WindowStyle == WindowStyle.SingleBorderWindow)
        {
            WindowState = WindowState.Maximized;
            WindowStyle = WindowStyle.None;
        }
        
        //AllowsTransparency = true; // 后台设置会导致程序崩溃
    }

    private void MenuExitButton_OnClick(object sender, RoutedEventArgs e)
    {
        this.Close();
        Environment.Exit(0);
    }

    private void MenuDarkModeButton_Click(object sender, RoutedEventArgs e)
    {
        var paletteHelper = new PaletteHelper();
        var theme = paletteHelper.GetTheme();

        theme.SetBaseTheme(DarkModeToggleButton.IsChecked == true ? Theme.Dark : Theme.Light);
        paletteHelper.SetTheme(theme);
    }

    private void OnSelectedItemChanged(object sender, DependencyPropertyChangedEventArgs e)
        => MainScrollViewer.ScrollToHome();
}
