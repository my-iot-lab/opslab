using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Ops.Engine.Scada.ViewModels;

namespace Ops.Engine.Scada.Views;

public partial class Login : Window
{
    public Login()
    {
        InitializeComponent();
        var ctx = App.Current.Services.GetService<LoginViewModel>();
        ctx!.SetWindow(this);

        DataContext = ctx;
    }
}
