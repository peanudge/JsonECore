using System.Text.Json;
using JsonECore;

namespace JsonECore.Tests.FunctionTests;

public class StringFunctionTests
{
    [Fact]
    public void Lowercase_MixedCase_ReturnsLowercase()
    {
        var result = JsonE.Render("""{"$eval": "lowercase('HeLLo')"}""", "{}");
        Assert.Equal("\"hello\"", result);
    }

    [Fact]
    public void Uppercase_MixedCase_ReturnsUppercase()
    {
        var result = JsonE.Render("""{"$eval": "uppercase('HeLLo')"}""", "{}");
        Assert.Equal("\"HELLO\"", result);
    }

    [Fact]
    public void Strip_WhitespaceString_ReturnsTrimmed()
    {
        var result = JsonE.Render("""{"$eval": "strip('  hello  ')"}""", "{}");
        Assert.Equal("\"hello\"", result);
    }

    [Fact]
    public void Lstrip_LeadingWhitespace_TrimsLeft()
    {
        var result = JsonE.Render("""{"$eval": "lstrip('  hello  ')"}""", "{}");
        Assert.Equal("\"hello  \"", result);
    }

    [Fact]
    public void Rstrip_TrailingWhitespace_TrimsRight()
    {
        var result = JsonE.Render("""{"$eval": "rstrip('  hello  ')"}""", "{}");
        Assert.Equal("\"  hello\"", result);
    }

    [Fact]
    public void Split_StringWithDelimiter_ReturnsArray()
    {
        var result = JsonE.Render("""{"$eval": "split('a,b,c', ',')"}""", "{}");
        Assert.Equal("[\"a\",\"b\",\"c\"]", result);
    }

    [Fact]
    public void Join_ArrayWithSeparator_ReturnsString()
    {
        var result = JsonE.Render("""{"$eval": "join(['a', 'b', 'c'], '-')"}""", "{}");
        Assert.Equal("\"a-b-c\"", result);
    }
}
