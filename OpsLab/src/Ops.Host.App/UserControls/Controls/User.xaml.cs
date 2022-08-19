using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ops.Host.App.ViewModels;

namespace Ops.Host.App.UserControls;

public partial class User : UserControl
{
    public User()
    {
        InitializeComponent();
        DataContext = App.Current.Services.GetRequiredService<UserViewModel>();
    }
}
