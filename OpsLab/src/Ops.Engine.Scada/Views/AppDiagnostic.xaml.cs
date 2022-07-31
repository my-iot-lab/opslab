using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ops.Engine.Scada.ViewModels;

namespace Ops.Engine.Scada.Views;

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
