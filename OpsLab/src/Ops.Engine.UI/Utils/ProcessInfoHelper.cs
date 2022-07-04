using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace Ops.Engine.UI.Utils;

/// <summary>
/// 进程管理信息帮助类。
/// </summary>
public static class ProcessInfoHelper
{
    /// <summary>
    /// 调用某个应用程序。
    /// 如通过 dotnet 启动，可设置 Start("dotnet", "xxx.dll", AppContext.BaseDirectory)；
    /// 或是 Start("xxx.exe", "", AppContext.BaseDirectory)。
    /// </summary>
    /// <param name="filename"></param>
    /// <param name="arguments"></param>
    /// <param name="workingDirectory"></param>
    public static void Start(string filename, string arguments = "", string workingDirectory = "")
    {
        ProcessStartInfo startInfo = new()
        {
            FileName = filename,
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
            RedirectStandardInput = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = true,
        };

        try
        {
            Process.Start(startInfo);
        }
        catch (Exception e) when (e is Win32Exception || e is FileNotFoundException)
        {
            //
        }
    }

    /// <summary>
    /// 强制关闭进程。
    /// </summary>
    /// <param name="processName">进程名称</param>
    /// <param name="isWindow">是否是窗体程序</param>
    public static void Kill(string processName, bool isWindow)
    {
        var processes = Process.GetProcessesByName(processName);

        try
        {
            foreach (var p in processes)
            {
                if (!p.HasExited)
                {
                    if (isWindow)
                    {
                        p.CloseMainWindow();
                    }
                    else
                    {
                        p.Kill();
                    }
                }
            }
        }
        catch (Exception e) when (e is Win32Exception || e is InvalidOperationException)
        {
            //
        }
    }
}
