using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ops.Engine.Scada.ViewModels;

namespace Ops.Engine.Scada.Views
{
    /// <summary>
    /// Interaction logic for Address.xaml
    /// </summary>
    public partial class Address : UserControl
    {
        public Address()
        {
            InitializeComponent();
            DataContext = App.Current.Services.GetService<AddressViewModel>();
        }
    }
}
