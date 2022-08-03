using System;
using Ops.Host.App.UserControls;

namespace Ops.Host.App;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        NonClientAreaContent = new NonClientAreaContent();
        ControlMain.Content = new MainWindowContent();
    }
}
