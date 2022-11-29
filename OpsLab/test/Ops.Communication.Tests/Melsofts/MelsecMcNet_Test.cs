using Ops.Communication.Profinet.Melsec;

namespace Ops.Communication.Tests.Melsofts;

public class MelsecMcNet_Test
{
    private const string IPAddress = "192.168.3.39";
    private const int Port = 4096;

    [Fact]
    public void Should_Read_Int_Test()
    {
        using var mc = new MelsecMcNet(IPAddress, Port);
        mc.ConnectServer();

        var ret1 = mc.ReadInt16("D100");
        Assert.True(ret1.IsSuccess, ret1.Message);
        Assert.True(ret1.Content == 3, ret1.Content.ToString());
    }

    [Fact]
    public void Should_Read_Float_Test()
    {
        using var mc = new MelsecMcNet(IPAddress, Port);
        mc.ConnectServer();

        var ret1 = mc.ReadFloat("D200");
        Assert.True(ret1.IsSuccess, ret1.Message);

        var v = Math.Round(ret1.Content, 3);
        Assert.True(v == 12.345d, v.ToString());
    }

    [Fact]
    public void Should_Write_Float_Test()
    {
        using var mc = new MelsecMcNet(IPAddress, Port);
        mc.ConnectServer();

        var ret1 = mc.Write("D200", 12.345f);
        Assert.True(ret1.IsSuccess, ret1.Message);
    }

    [Fact]
    public void Should_Read_String_Test()
    {
        using var mc = new MelsecMcNet(IPAddress, Port);
        mc.ConnectServer();

        var ret1 = mc.ReadString("D300", 20);
        Assert.True(ret1.IsSuccess, ret1.Message);

        var message1 = ret1.Content.Trim('\0');
        Assert.True(message1 == "abcd1234", message1);
    }

    [Fact]
    public void Should_Write_String_Test()
    {
        using var mc = new MelsecMcNet(IPAddress, Port);
        mc.ConnectServer();

        var ret1 = mc.Write("D300", "abcd1234");
        Assert.True(ret1.IsSuccess, ret1.Message);
    }
}
