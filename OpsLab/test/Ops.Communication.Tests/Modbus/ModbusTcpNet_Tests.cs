using Ops.Communication.Modbus;

namespace Ops.Communication.Tests.Modbus;

public class ModbusTcpNet_Tests
{
    [Fact]
    public async Task Should_WriteAsync_Short_Test()
    {
        using var driver = new ModbusTcpNet("127.0.0.1");
        var result = await driver.WriteAsync("s=1;x=3;2", (short)0);
        Assert.True(result.IsSuccess, result.Message);
    }
}
