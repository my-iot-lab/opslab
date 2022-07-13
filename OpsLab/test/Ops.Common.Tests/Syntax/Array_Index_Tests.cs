namespace Ops.Common.Tests.Syntax;

public class Array_Index_Tests
{
    [Fact]
    public void Should_Split_Index_Test()
    {
        string arr0 = "12345abcde";

        string arr1 = arr0[5..];
        Assert.Equal("abcde", arr1);

        string arr2 = arr0[10..];
        Assert.Equal("", arr2);
    }
}
