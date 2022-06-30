namespace Ops.Common.Tests.Syntax;

public class DataConvert_Tests
{
    [Fact]
    public void Should_Convert_Object_To_Array_Test()
    {
        object arr = new int[] { 1, 3, 5, 7, 9 };
        Assert.True(arr.GetType().Name == "Int32[]", arr.GetType().Name);

        var arr2 = (int[])arr;
        Assert.True(arr2.GetType().Name == "Int32[]", arr2.GetType().Name);
    }

    [Fact]
    public void Should_Convert_Object_To_Int16_Test()
    {
        object obj = (short)5;

        var obj2 = (short)obj;
        Assert.True(obj2 == 5, obj2.ToString());
    }
}
