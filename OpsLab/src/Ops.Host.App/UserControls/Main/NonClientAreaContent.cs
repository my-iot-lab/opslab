using Microsoft.Extensions.DependencyInjection;
using Ops.Host.App.ViewModels;

namespace Ops.Host.App.UserControls;

public partial class NonClientAreaContent
{
    public NonClientAreaContent()
    {
        InitializeComponent();
        this.DataContext = App.Current.Services.GetService<NonClientAreaContentViewModel>();
    }
}
