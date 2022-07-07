using System.Text.Json;
using Ops.Exchange.Utils;

namespace Ops.Exchange.Tests.Utils;

public class Converter_Tests
{
    [Fact]
    public void Should_Convert_Object_To_Array_Test()
    {
        var obj1 = new int[] { 1, 2, 3 };
        var arr1 = Object2ValueHelper.ToArray<int>(obj1);
        Assert.True(arr1.Length > 0);
    }

    [Fact]
    public void Should_Json_Convert_Object_To_Array_Test()
    {
        var obj1 = new { 
            Code = 3,
            BoolTrue = true,
            BoolFalse = false,
            ShortArr = new short[] { 1, 2, 3 },
            IntArr = new int[] { 1, 2, 3 },
            FloatArr = new float[] { 1.345F, 2.1F, 3F },
            DoubleArr = new double[] { 11.345, 12.2, 13 },
        };
        var json = JsonSerializer.Serialize(obj1);
        var objA = JsonSerializer.Deserialize<ConvertModel>(json);
        Assert.NotNull(objA);

        var boolTrueObj = Object2ValueHelper.To<bool>(objA.BoolTrue);
        Assert.True(boolTrueObj);

        var boolFalseObj = Object2ValueHelper.To<bool>(objA.BoolFalse);
        Assert.True(!boolFalseObj);

        var shortArr = Object2ValueHelper.ToArray<short>(objA.ShortArr);
        Assert.True(shortArr.Length > 0);

        var intArr = Object2ValueHelper.ToArray<int>(objA.IntArr);
        Assert.True(intArr.Length > 0);

        var floatArr = Object2ValueHelper.ToArray<float>(objA.FloatArr);
        Assert.True(floatArr.Length > 0);

        var doubleArr = Object2ValueHelper.ToArray<double>(objA.DoubleArr);
        Assert.True(doubleArr.Length > 0);
    }

    internal class ConvertModel
    {
        public short Code { get; set; }

        public object BoolTrue { get; set; }

        public object BoolFalse { get; set; }

        public object ShortArr { get; set; }

        public object IntArr { get; set; }

        public object FloatArr { get; set; }

        public object DoubleArr { get; set; }
    }
}
