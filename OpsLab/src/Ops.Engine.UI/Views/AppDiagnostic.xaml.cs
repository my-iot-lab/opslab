using System.Windows.Controls;

using Microsoft.Extensions.DependencyInjection;
using Ops.Engine.UI.ViewModels;

namespace Ops.Engine.UI.Views;

/// <summary>
/// Interaction logic for AppDiagnostic.xaml
/// </summary>
public partial class AppDiagnostic : UserControl
{
    public AppDiagnostic()
    {
        InitializeComponent();
        DataContext = App.Current.Services.GetService<AppDiagnosticViewModel>();
    }
}
