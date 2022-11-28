using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ops.Engine.Scada.ViewModels;

namespace Ops.Engine.Scada.Views;

public partial class Home2 : UserControl
{
    public Home2()
    {
        InitializeComponent();
        DataContext = App.Current.Services.GetService<Home2ViewModel>();
    }
}
