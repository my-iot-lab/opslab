using System;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Ops.Engine.UI.ViewModels;

namespace Ops.Engine.UI.Views;

public partial class Home : UserControl
{
    public Home()
    {
        InitializeComponent();

        var vm = App.Current.Services.GetRequiredService<HomeViewModel>();
        vm.MessageAutoScrollDelegate = () =>
        {
            this.MessageListBox.ScrollIntoView(this.MessageListBox.Items[this.MessageListBox.Items.Count - 1]);
        };
        vm.MessageTipDelegate = msg =>
        {
            this.SnackbarTip.MessageQueue?.Enqueue(
                msg,
                null,
                null,
                null,
                false,
                true,
                TimeSpan.FromSeconds(5));
        };
        DataContext = vm;

        // 设置定时器
        if (vm.TimerHandler != null)
        {
            var timer = new DispatcherTimer();
            timer.Tick += vm.TimerHandler;
            timer.Interval = TimeSpan.FromSeconds(2);
            timer.Start();
        }
    }
}
