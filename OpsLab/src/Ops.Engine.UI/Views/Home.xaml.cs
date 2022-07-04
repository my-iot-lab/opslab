using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ops.Engine.UI.ViewModels;

namespace Ops.Engine.UI.Views;

public partial class Home : UserControl
{
    public Home()
    {
        InitializeComponent();
        DataContext = App.Current.Services.GetService<HomeViewModel>();
    }
}
