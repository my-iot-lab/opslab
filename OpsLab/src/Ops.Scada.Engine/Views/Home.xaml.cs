using System.Windows;
using System.Windows.Controls;
using Ops.Scada.Engine.Utils;

namespace Ops.Scada.Engine.Views;

public partial class Home : UserControl
{
    public Home() => InitializeComponent();

    private void GitHubButton_OnClick(object sender, RoutedEventArgs e)
        => Link.OpenInBrowser("https://github.com/ButchersBoy/MaterialDesignInXamlToolkit");

    private void TwitterButton_OnClick(object sender, RoutedEventArgs e)
        => Link.OpenInBrowser("https://twitter.com/James_Willock");

    private void ChatButton_OnClick(object sender, RoutedEventArgs e)
        => Link.OpenInBrowser("https://gitter.im/ButchersBoy/MaterialDesignInXamlToolkit");

    private void EmailButton_OnClick(object sender, RoutedEventArgs e)
        => Link.OpenInBrowser("mailto://james@dragablz.net");

    private void DonateButton_OnClick(object sender, RoutedEventArgs e)
        => Link.OpenInBrowser("https://opencollective.com/materialdesigninxaml");
}
