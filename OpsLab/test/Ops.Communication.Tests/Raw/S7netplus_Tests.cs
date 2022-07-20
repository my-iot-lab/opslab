using S7.Net;

namespace Ops.Communication.Tests.Raw;

public class S7netplus_Tests
{
    [Fact]
    public void Should_Read_Int_Test()
    {
        using var s7 = new Plc(CpuType.S71500, "192.168.0.1", 0, 1);
        s7.Open();

        //var val1 = s7.Read(DataType.DataBlock, 4, 0, VarType.Int, 0);
        //Assert.NotNull(val1);

        ushort val2 = (ushort)s7.Read("DB4.DBW0");
        Assert.True(val2 == 1, val2.ToString());
    }
}
