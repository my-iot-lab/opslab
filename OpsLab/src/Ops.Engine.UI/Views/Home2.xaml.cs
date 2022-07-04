using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ops.Engine.UI.ViewModels;

namespace Ops.Engine.UI.Views;

public partial class Home2 : UserControl
{
    public Home2()
    {
        InitializeComponent();
        DataContext = App.Current.Services.GetService<Home2ViewModel>();
    }
}
