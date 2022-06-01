using System.Threading.Tasks;
using Xunit;
using NetMQ;
using NetMQ.Sockets;

namespace Ops.Exchange.Tests.MQ;

public class MQ_Request_Response_Tests
{
    [Fact]
    public void Should_Request_Response_Test()
    {
        using var responseSocket = new ResponseSocket("@tcp://*:5555"); // @ 绑定本机地址（仅本地地址）
        using var requestSocket = new RequestSocket(">tcp://localhost:5555"); // > 连接到指定的地址

        requestSocket.SendFrame("Hello");
        var message1 = responseSocket.ReceiveFrameString();
        Assert.True(message1.Equals("Hello"));

        responseSocket.SendFrame("World");
        var message2 = requestSocket.ReceiveFrameString();
        Assert.True(message2.Equals("World"));
    }

    [Fact]
    public async void Should_Push_Pull_Test()
    {
        using var sender = new PushSocket("@tcp://localhost:5557");
        using var receiver = new PullSocket(">tcp://localhost:5557");

        int num = 0;
        _ = Task.Run(() =>
        {
            for (int i = 0; i < 10; i++)
            {
                var workload = receiver.ReceiveFrameString();
                num += int.Parse(workload);
            }
        });

        await Task.Delay(500);

        _ = Task.Run(async () =>
        {
            for (int i = 0; i < 10; i++)
            {
                sender.SendFrame(i.ToString());

                await Task.Delay(100);
            }
        });

        await Task.Delay(2000);
        Assert.True(num == 45, num.ToString());
    }
}
