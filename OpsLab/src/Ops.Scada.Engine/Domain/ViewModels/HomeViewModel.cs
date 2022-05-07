using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Ops.Scada.Engine.Utils;

namespace Ops.Scada.Engine.Domain.ViewModels;

public class HomeViewModel : ObservableObject
{
    public ICommand GitHubCommand { get; } = new RelayCommand(() 
        => Link.OpenInBrowser("https://github.com/ButchersBoy/MaterialDesignInXamlToolkit"));

    public ICommand TwitterCommand { get; } = new RelayCommand(()
        => Link.OpenInBrowser("https://twitter.com/James_Willock"));

    public ICommand ChatCommand { get; } = new RelayCommand(()
        => Link.OpenInBrowser("https://gitter.im/ButchersBoy/MaterialDesignInXamlToolkit"));

    public ICommand EmailCommand { get; } = new RelayCommand(()
        => Link.OpenInBrowser("mailto://james@dragablz.net"));

    public ICommand DonateCommand { get; } = new RelayCommand(()
        => Link.OpenInBrowser("https://opencollective.com/materialdesigninxaml"));
}