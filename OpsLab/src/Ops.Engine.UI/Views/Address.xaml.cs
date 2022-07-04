using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Ops.Engine.UI.Domain.ViewModels;

namespace Ops.Engine.UI.Views
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
