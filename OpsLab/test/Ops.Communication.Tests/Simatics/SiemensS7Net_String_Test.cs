using Ops.Communication.Profinet.Siemens;

namespace Ops.Communication.Tests.Simatics;

public class SiemensS7Net_String_Test
{
    private const string IPAddress = "192.168.120.10";

    [Fact]
    public async Task Should_Read_String_Test()
    {
        using var s7 = new SiemensS7Net(SiemensPLCS.S1500, IPAddress);
        await s7.ConnectServerAsync();

        var ret1 = await s7.ReadStringAsync("DB3012.14"); // DB3012.12、DB3012.14
        Assert.True(ret1.IsSuccess, ret1.Message);
        Assert.True(ret1.Content == "ABC123", ret1.Content);
    }
}
