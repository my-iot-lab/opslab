using Ops.Communication.Profinet.Siemens;

namespace Ops.Communication.Tests.Simatics;

public class SiemensS7Net_Test
{
    [Fact]
    public void Should_Read_Int_Test()
    {
        using var s7 = new SiemensS7Net(SiemensPLCS.S1500, "192.168.0.1");
        s7.ConnectServer();

        var ret1 =  s7.ReadInt16("DB4.0");
        Assert.True(ret1.IsSuccess, ret1.Message);
        Assert.True(ret1.Content == 1, ret1.Content.ToString());
    }
}
