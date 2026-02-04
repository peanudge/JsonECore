using System.Text.Json;
using JsonECore;

namespace JsonECore.Tests.FunctionTests;

public class TypeFunctionTests
{
    [Theory]
    [InlineData("""{"$eval": "typeof(123)"}""", "\"number\"")]
    [InlineData("""{"$eval": "typeof('hello')"}""", "\"string\"")]
    [InlineData("""{"$eval": "typeof(true)"}""", "\"boolean\"")]
    [InlineData("""{"$eval": "typeof(null)"}""", "\"null\"")]
    [InlineData("""{"$eval": "typeof([1, 2])"}""", "\"array\"")]
    [InlineData("""{"$eval": "typeof({a: 1})"}""", "\"object\"")]
    public void Typeof_ReturnsCorrectType(string template, string expected)
    {
        var result = JsonE.Render(template, "{}");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Str_Number_ReturnsString()
    {
        var result = JsonE.Render("""{"$eval": "str(123)"}""", "{}");
        Assert.Equal("\"123\"", result);
    }

    [Fact]
    public void Str_Boolean_ReturnsString()
    {
        var result = JsonE.Render("""{"$eval": "str(true)"}""", "{}");
        Assert.Equal("\"true\"", result);
    }

    [Fact]
    public void Number_StringNumber_ReturnsNumber()
    {
        var result = JsonE.Render("""{"$eval": "number('123')"}""", "{}");
        Assert.Equal("123", result);
    }

    [Fact]
    public void Number_Boolean_ReturnsNumber()
    {
        var result = JsonE.Render("""{"$eval": "number(true)"}""", "{}");
        Assert.Equal("1", result);
    }

    [Fact]
    public void Len_String_ReturnsLength()
    {
        var result = JsonE.Render("""{"$eval": "len('hello')"}""", "{}");
        Assert.Equal("5", result);
    }

    [Fact]
    public void Len_Array_ReturnsLength()
    {
        var result = JsonE.Render("""{"$eval": "len([1, 2, 3])"}""", "{}");
        Assert.Equal("3", result);
    }

    [Fact]
    public void Len_Object_ReturnsKeyCount()
    {
        var result = JsonE.Render("""{"$eval": "len({a: 1, b: 2})"}""", "{}");
        Assert.Equal("2", result);
    }
}
