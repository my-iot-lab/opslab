using Ops.Communication.Modbus;

namespace Ops.Communication.Tests.Modbus;

public class ModbusTcpNet_Tests
{
    [Fact]
    public async Task Should_WriteAsync_Short_Test()
    {
        using var driver = new ModbusTcpNet("127.0.0.1");
        var result = await driver.WriteAsync("s=1;x=3;2", (short)4);
        Assert.True(result.IsSuccess, $"ErrorCode:{result.ErrorCode}, Message:{result.Message}");
    }

    [Fact]
    public async Task Should_ReadAsync_Short_Test()
    {
        using var driver = new ModbusTcpNet("127.0.0.1");
        await driver.ConnectServerAsync();
        var result = await driver.ReadInt16Async("s=1;x=3;2");
        Assert.True(result.IsSuccess, $"ErrorCode:{result.ErrorCode}, Message:{result.Message}");
    }
}
