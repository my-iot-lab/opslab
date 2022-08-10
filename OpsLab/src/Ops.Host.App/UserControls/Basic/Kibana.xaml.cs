using System;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Extensions.DependencyInjection;
using Ops.Host.App.ViewModels;

namespace Ops.Host.App.UserControls;

public partial class Kibana : UserControl
{
    public Kibana()
    {
        InitializeComponent();
        var vm = App.Current.Services.GetRequiredService<KibanaViewModel>();
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
