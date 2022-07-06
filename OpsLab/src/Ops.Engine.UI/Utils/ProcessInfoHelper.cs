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
    /// 检查指定的进程是否有在运行。
    /// </summary>
    /// <param name="processName">指定的进程名称</param>
    /// <returns></returns>
    public static bool IsRunning(string processName)
    {
        var processes = Process.GetProcessesByName(processName);
        return processes.Length > 0;
    }

    /// <summary>
    /// 调用某个应用程序。
    /// 如通过 dotnet 启动，可设置 Start("dotnet", "xxx.dll", AppContext.BaseDirectory)；
    /// 或是 Start("xxx.exe", "", AppContext.BaseDirectory)。
    /// 注：需自定义处理错误代码。
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

        Process.Start(startInfo);
    }

    /// <summary>
    /// 强制关闭进程。
    /// 注：需自定义处理错误代码。
    /// </summary>
    /// <param name="processName">进程名称</param>
    /// <param name="isWindow">是否是窗体程序</param>
    public static void Kill(string processName, bool isWindow)
    {
        var processes = Process.GetProcessesByName(processName);
        foreach (var ps in processes)
        {
            if (!ps.HasExited)
            {
                if (isWindow)
                {
                    ps.CloseMainWindow();
                }
                else
                {
                    ps.Kill();
                }
            }
        }
    }

    public static void Kill(int processId, bool isWindow)
    {
        var ps = Process.GetProcessById(processId);
        if (!ps.HasExited)
        {
            if (isWindow)
            {
                ps.CloseMainWindow();
            }
            else
            {
                ps.Kill();
            }
        }
    }
}
