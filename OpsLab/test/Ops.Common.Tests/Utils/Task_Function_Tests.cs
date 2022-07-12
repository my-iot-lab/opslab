using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace Ops.Common.Tests.Utils;

public class Task_Function_Tests
{
    [Fact]
    public void Should_Ping_Test()
    {
        Ping ping = new();
        var pingReply = ping.Send("192.168.1.199", 1000);
        Assert.True(pingReply.Status != IPStatus.Success);
    }

    [Fact]
    public void Should_WaitAll_Task_Test()
    {
        int num = 1;
        List<Task> tasks = new(num);
        for (int i = 0; i < num; i++)
        {
            tasks.Add(new Task(() =>
            {
                Ping ping = new();
                var pingReply = ping.Send("192.168.1.199", 1000);
            }));
        }

        var result = Task.WaitAll(tasks.ToArray(), 5000);
        Assert.True(result);
    }
}
