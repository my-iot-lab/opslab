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

    [Fact]
    public void Should_ConvertData_Test()
    {
        object obj1 = "";
        Assert.True(obj1 is string, obj1.GetType().Name); // true

        object obj2 = true;
        Assert.True(obj2 is bool, obj2.GetType().Name); // true

        object obj3 = 2;
        Assert.True(obj3 is int, obj3.GetType().Name); // true

        object obj4 = 123;
        Assert.True(obj4 is long, obj4.GetType().Name); // false -- Int32

        object obj5 = 12.3;
        Assert.True(obj5 is double, obj5.GetType().Name); // true
    }
}
