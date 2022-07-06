using System.Diagnostics;
using System.IO;

namespace Ops.Common.Tests.Utils;

public class ProcessHelper_Tests
{
    [Fact]
    public void Should_Process_IsRunning_Test()
    {
        var processName = "Ops.Engine.App.exe";
        var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(processName));
        Assert.True(processes.Length > 0, processes.Length.ToString());
    }

    [Fact]
    public void Should_Process_Startup_Test()
    {
        var dir = @"E:\Github\opslab\OpsLab\src\App\Ops.Engine.App\bin\Release\net6.0\publish\";

        ProcessStartInfo startInfo = new()
        {
            FileName = dir + "Ops.Engine.App.exe",
            Arguments = "",
            WorkingDirectory = dir,
            UseShellExecute = false,
            RedirectStandardInput = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = true,
        };

        var ps = Process.Start(startInfo);
        Assert.NotNull(ps);
    }

    [Fact]
    public void Should_Process_Close_NotMainWindow_Test()
    {
        var processName = "Ops.Engine.App.exe";
        var processes = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(processName));

        foreach (var p in processes)
        {
            if (!p.HasExited)
            {
                p.Kill();
            }
        }
    }
}
