using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace Ops.Engine.UI.Domain.ViewModels;

public class LoginViewModel : ObservableObject
{
    private Window? _window;

    private string _userName = string.Empty;
    private string _password = string.Empty;
    private string? _errMessage;

    public LoginViewModel()
    {
        SignCommand = new RelayCommand(async () =>
        {
            await Login();
        });

        ExitCommand = new RelayCommand(() =>
        {
            Exit();
        });
    }

    #region 绑定属性

    public string UserName
    {
        get => _userName;
        set { SetProperty(ref _userName, value); }
    }

    public string Password
    {
        get => _password;
        set { SetProperty(ref _password, value); }
    }

    public string? ErrMessage
    {
        get => _errMessage;
        set { SetProperty(ref _errMessage, value); }
    }

    #endregion

    #region Binding Command

    /// <summary>
    /// 登录
    /// </summary>
    public ICommand SignCommand { get; }

    /// <summary>
    /// 退出
    /// </summary>
    public ICommand ExitCommand { get; }

    #endregion

    #region Public 

    /// <summary>
    /// 设置当前窗体。
    /// </summary>
    /// <param name="window">要设置的窗体。</param>
    public void SetWindow(Window window)
    {
        if (_window == null)
        {
            _window = window;
        }
    }

    #endregion

    #region Login/Exit

    private Task Login()
    {
        ErrMessage = $"{UserName}/{Password}";

        new MainWindow().Show();
        _window?.Close();

        return Task.CompletedTask;
    }

    private void Exit()
    {
        _window?.Close();
    }

    #endregion
}
