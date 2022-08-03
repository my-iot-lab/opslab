using Microsoft.Extensions.DependencyInjection;
using Ops.Host.App.ViewModels;

namespace Ops.Host.App.UserControls;

public partial class MainWindowContent
{
    public MainWindowContent()
    {
        InitializeComponent();
        this.DataContext = App.Current.Services.GetService<MainWindowViewModel>();
    }
}
