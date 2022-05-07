using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ops.Scada.Engine.Domain.ViewModels;

namespace Ops.Scada.Engine.Views;

public partial class Home : UserControl
{
    public Home()
    {
        InitializeComponent();
        DataContext = App.Current.Services.GetService<HomeViewModel>();
    }
}
