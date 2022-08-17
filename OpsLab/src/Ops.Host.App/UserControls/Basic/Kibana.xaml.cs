using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ops.Host.App.ViewModels;

namespace Ops.Host.App.UserControls;

public partial class Kibana : UserControl
{
    public Kibana()
    {
        InitializeComponent();
        DataContext = App.Current.Services.GetRequiredService<KibanaViewModel>();
    }
}
