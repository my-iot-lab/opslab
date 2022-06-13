using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ops.Engine.UI.Utils;

internal static class Link
{
    /// <summary>
    /// 在浏览器中打开指定的连接。
    /// </summary>
    /// <param name="url">要打开的连接</param>
    public static void OpenInBrowser(string? url)
    {
        if (url is null)
        {
            return;
        }

        // https://github.com/dotnet/corefx/issues/10361
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            url = url.Replace("&", "^&");
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}")
            {
                CreateNoWindow = true,
            });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", url);
        }
    }
}
