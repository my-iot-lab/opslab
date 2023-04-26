using Ops.Communication.Profinet.Siemens;
using System.Text;

namespace Ops.Communication.Tests.Simatics;

public class SiemensS7Net_Test
{
    private const string IPAddress = "192.168.0.1";

    [Fact]
    public void Should_Read_Int_Test()
    {
        using var s7 = new SiemensS7Net(SiemensPLCS.S1500, IPAddress);
        s7.ConnectServer();

        var ret1 =  s7.ReadInt16("DB59.2");
        Assert.True(ret1.IsSuccess, ret1.Message);
        Assert.True(ret1.Content == 1, ret1.Content.ToString());
    }

    [Fact]
    public void Should_Read_String_Test()
    {
        using var s7 = new SiemensS7Net(SiemensPLCS.S1500, IPAddress);
        s7.ConnectServer();

        var ret1 = s7.ReadString("DB4.30"); // s7.ReadString("DB4.30", 20);
        Assert.True(ret1.IsSuccess, ret1.Message);
        Assert.True(ret1.Content == "SN1234567890", ret1.Content);
    }

    [Fact]
    public void Should_Write_String_Test()
    {
        using var s7 = new SiemensS7Net(SiemensPLCS.S1500, IPAddress);
        s7.ConnectServer();

        var ret1 = s7.Write("DB3.40", "Hello");
        Assert.True(ret1.IsSuccess, ret1.Message);
    }

    [Fact]
    public void Should_Write_WString_Test()
    {
        using var s7 = new SiemensS7Net(SiemensPLCS.S1500, IPAddress);
        s7.ConnectServer();

        var ret1 = s7.WriteWString("DB3.58", " 你好世界。");
        Assert.True(ret1.IsSuccess, ret1.Message);
    }

    [Fact]
    public void Should_Read_Bool_Test()
    {
        using var s7 = new SiemensS7Net(SiemensPLCS.S1500, IPAddress);
        s7.ConnectServer();

        var ret1 = s7.ReadBool("DB5.100", 100);
        Assert.True(ret1.IsSuccess, ret1.Message);
    }

    [Fact]
    public void Should_Read_Byte_Test()
    {
        using var s7 = new SiemensS7Net(SiemensPLCS.S1500, IPAddress);
        s7.ConnectServer();

        var ret1 = s7.Read("DB5.0", 100);
        Assert.True(ret1.IsSuccess, ret1.Message);
    }

    [Fact]
    public async Task Should_Read_Multiple_Address1_Test()
    {
        using var s7 = new SiemensS7Net(SiemensPLCS.S1500, "192.168.0.1");
        await s7.ConnectServerAsync();

        //string[] addresses = { "DB101.414", "DB101.456", "DB101.458", "DB101.500", "DB101.504", "DB101.506", "DB101.510", "DB101.630", "DB101.750" };
        //ushort[] lengths = { 42, 2, 42, 4, 2, 4, 120, 120, 120 }; // 类型宽度

        string[] addresses = { "DB101.414" };
        ushort[] lengths = { 462 }; // 类型宽度（462 byte）

        var ret2 = await s7.ReadAsync(addresses, lengths);

        //var ret07 = s7.ByteTransform.TransString(ret2.Content, 98, 40, Encoding.ASCII).TrimEnd('\0'); // 92+4+2
        //var ret08 = s7.ByteTransform.TransString(ret2.Content, 2, 40, Encoding.ASCII).TrimEnd('\0'); // 96+
        //var ret09 = s7.ByteTransform.TransString(ret2.Content, 2, 40, Encoding.ASCII).TrimEnd('\0');

        //var ret07 = ret2.Content[290];
        //var ret07_1 = (ret07 & 0x01) == 0x01; // 0000_0001
        //var ret07_2 = (ret07 & 0x02) == 0x02; // 0000_0010
        //var ret07_3 = (ret07 & 0x04) == 0x04; // 0000_0100


        Assert.True(ret2.IsSuccess);
    }

    [Fact]
    public async Task Should_Read_Multiple_Address2_Test()
    {
        using var s7 = new SiemensS7Net(SiemensPLCS.S1500, "192.168.0.1");
        await s7.ConnectServerAsync();

        //string[] addresses = { "DB101.414", "DB101.456", "DB101.458", "DB101.500", "DB101.504", "DB101.506", "DB101.510", "DB101.630", "DB101.750" };
        //ushort[] lengths = { 42, 2, 42, 4, 2, 4, 120, 120, 120 }; // 类型宽度

        string[] addresses = { "DB101.414", "DB101.456", "DB101.458", "DB101.500", "DB101.504", "DB101.506", "DB101.510", "DB101.630", "DB101.750" };
        ushort[] lengths = { 42, 2, 42, 4, 2, 4, 120, 120, 94 }; // 类型宽度（430 byte）

        var ret2 = await s7.ReadAsync(addresses, lengths);
        var ret01 = s7.ByteTransform.TransString(ret2.Content, 2, 40, Encoding.ASCII).TrimEnd('\0'); // 0+2
        var ret02 = s7.ByteTransform.TransInt16(ret2.Content, 42); // 42
        var ret03 = s7.ByteTransform.TransString(ret2.Content, 46, 40, Encoding.ASCII); // 42+2+2
        var ret04 = s7.ByteTransform.TransSingle(ret2.Content, 86); // 44+42
        var ret05 = s7.ByteTransform.TransInt16(ret2.Content, 90); // 86+4
        var ret06 = s7.ByteTransform.TransSingle(ret2.Content, 92); // 90 + 2

        //var ret07 = s7.ByteTransform.TransString(ret2.Content, 98, 40, Encoding.ASCII).TrimEnd('\0'); // 92+4+2
        //var ret08 = s7.ByteTransform.TransString(ret2.Content, 2, 40, Encoding.ASCII).TrimEnd('\0'); // 96+
        //var ret09 = s7.ByteTransform.TransString(ret2.Content, 2, 40, Encoding.ASCII).TrimEnd('\0');

        //var ret07 = ret2.Content[290];
        //var ret07_1 = (ret07 & 0x01) == 0x01; // 0000_0001
        //var ret07_2 = (ret07 & 0x02) == 0x02; // 0000_0010
        //var ret07_3 = (ret07 & 0x04) == 0x04; // 0000_0100


        Assert.True(ret2.IsSuccess);
    }
}
