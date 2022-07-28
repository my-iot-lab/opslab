using System;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MaterialDesignThemes.Wpf;
using Ops.Engine.UI.ViewModels;
using Ops.Engine.UI.Utils;
using Ops.Engine.UI.Config;

namespace Ops.Engine.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var vm = App.Current.Services.GetRequiredService<MainViewModel>();
        DataContext = vm;

        var paletteHelper = new PaletteHelper();
        var theme = paletteHelper.GetTheme();

        DarkModeToggleButton.IsChecked = theme.GetBaseTheme() == BaseTheme.Dark;

        if (paletteHelper.GetThemeManager() is { } themeManager)
        {
            themeManager.ThemeChanged += (_, e)
                => DarkModeToggleButton.IsChecked = e.NewTheme?.GetBaseTheme() == BaseTheme.Dark;
        }

        // 设置定时器
        if (vm.TimerHandler != null)
        {
            var timer = new DispatcherTimer();
            timer.Tick += vm.TimerHandler;
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Start();
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

    #region 事件

    /// <summary>
    /// 点检菜单栏（非滚动条）后自动显示界面内容。
    /// </summary>
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

    public void MenuOpenWebServer_OnClick(object sender, RoutedEventArgs e)
    {
        var uiOptions = App.Current.Services.GetRequiredService<IOptions<OpsUIOptions>>().Value;
        var(ok, err) = OpenWebServer(uiOptions.WebServerEntryPath);
        if (!ok)
        {
            this.SnackbarTip.MessageQueue?.Enqueue(err, null, null, null, false, true, TimeSpan.FromSeconds(5));
        }
    }

    public void MenuCloseWebServer_OnClick(object sender, RoutedEventArgs e)
    {
        var uiOptions = App.Current.Services.GetRequiredService<IOptions<OpsUIOptions>>().Value;
        var (ok, err) = CloseWebServer(uiOptions.WebServerEntryPath);
        if (!ok)
        {
            this.SnackbarTip.MessageQueue?.Enqueue(err, null, null, null, false, true, TimeSpan.FromSeconds(5));
        }
    }

    #endregion

    private void OnSelectedItemChanged(object sender, DependencyPropertyChangedEventArgs e)
        => MainScrollViewer.ScrollToHome();

    /// <summary>
    /// 打开后台
    /// </summary>
    private (bool ok, string err) OpenWebServer(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return (false, "没有配置服务路径");
        }

        try
        {
            var fullpath = Path.GetFullPath(path, AppContext.BaseDirectory);
            var processName = Path.GetFileNameWithoutExtension(fullpath);
            if (ProcessInfoHelper.IsRunning(processName))
            {
                return (false, "服务已在运行中");
            }

            ProcessInfoHelper.Start(fullpath, workingDirectory: Path.GetDirectoryName(fullpath)!);
        }
        catch (Exception)
        {
            return (false, $"服务启动失败");
        }

        return (true, "");
    }

    private (bool, string) CloseWebServer(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return (false, "没有配置服务路径");
        }

        try
        {
            var processName = Path.GetFileNameWithoutExtension(path);
            ProcessInfoHelper.Kill(processName, false);
        }
        catch (Exception)
        {
            return (false, $"服务关闭失败");
        }

        return (true, "");
    }
}
