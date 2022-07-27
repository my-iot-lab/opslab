namespace BootstrapAdmin.Web.Services.SMS;

internal class AutoExpireValidateCode
{
    public AutoExpireValidateCode(string phone, string code, TimeSpan expires, Action<string> expiredCallback)
    {
        Phone = phone;
        Code = code;
        Expires = expires;
        ExpiredCallback = expiredCallback;
        RunAsync();
    }

    protected Action<string> ExpiredCallback { get; set; }

    public string Code { get; private set; }

    public string Phone { get; }

    public TimeSpan Expires { get; set; }

    private CancellationTokenSource? _tokenSource;

    private Task RunAsync() => Task.Run(() =>
    {
        _tokenSource = new CancellationTokenSource();
        if (!_tokenSource.Token.WaitHandle.WaitOne(Expires)) ExpiredCallback(Phone);
    });

    public AutoExpireValidateCode Reset(string code)
    {
        Code = code;
        _tokenSource?.Cancel();
        RunAsync();
        return this;
    }
}
