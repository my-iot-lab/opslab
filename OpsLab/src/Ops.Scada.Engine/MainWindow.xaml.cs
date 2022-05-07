using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using MaterialDesignThemes.Wpf;
using Ops.Scada.Engine.Domain;
using Ops.Scada.Engine.UIControls;
using Ops.Scada.Engine.Domain.ViewModels;

namespace Ops.Scada.Engine;

public partial class MainWindow : Window
{
    public static Snackbar Snackbar = new();
    public MainWindow()
    {
        InitializeComponent();

        Task.Factory.StartNew(() => Thread.Sleep(2500)).ContinueWith(t =>
        {
            MainSnackbar.MessageQueue?.Enqueue("Welcome to Material Design In XAML Tookit");
        }, TaskScheduler.FromCurrentSynchronizationContext());

        //DataContext = new MainWindowViewModel(MainSnackbar.MessageQueue!);
        DataContext = App.Current.Services.GetService<MainViewModel>();

        var paletteHelper = new PaletteHelper();
        var theme = paletteHelper.GetTheme();

        DarkModeToggleButton.IsChecked = theme.GetBaseTheme() == BaseTheme.Dark;

        if (paletteHelper.GetThemeManager() is { } themeManager)
        {
            themeManager.ThemeChanged += (_, e)
                => DarkModeToggleButton.IsChecked = e.NewTheme?.GetBaseTheme() == BaseTheme.Dark;
        }

        Snackbar = MainSnackbar;
    }

    private void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        //until we had a StaysOpen glag to Drawer, this will help with scroll bars
        var dependencyObject = Mouse.Captured as DependencyObject;

        while (dependencyObject != null)
        {
            if (dependencyObject is ScrollBar) return;
            dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
        }

        MenuToggleButton.IsChecked = false;
    }

    private async void MenuPopupButton_OnClick(object sender, RoutedEventArgs e)
    {
        var sampleMessageDialog = new SampleMessageDialog
        {
            Message = { Text = ((ButtonBase)sender).Content.ToString() }
        };

        await DialogHost.Show(sampleMessageDialog, "RootDialog");
    }

    private void OnCopy(object sender, ExecutedRoutedEventArgs e)
    {
        if (e.Parameter is string stringValue)
        {
            try
            {
                Clipboard.SetDataObject(stringValue);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }
    }

    private void MenuToggleButton_OnClick(object sender, RoutedEventArgs e)
        => DemoItemsSearchBox.Focus();

    private void MenuDarkModeButton_Click(object sender, RoutedEventArgs e)
        => ModifyTheme(DarkModeToggleButton.IsChecked == true);

    private static void ModifyTheme(bool isDarkTheme)
    {
        var paletteHelper = new PaletteHelper();
        var theme = paletteHelper.GetTheme();

        theme.SetBaseTheme(isDarkTheme ? Theme.Dark : Theme.Light);
        paletteHelper.SetTheme(theme);
    }

    private void OnSelectedItemChanged(object sender, DependencyPropertyChangedEventArgs e)
        => MainScrollViewer.ScrollToHome();
}
