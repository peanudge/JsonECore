using System.Text.Json;
using JsonECore;

namespace JsonECore.Tests;

public class InterpolationTests
{
    [Fact]
    public void StringInterpolation_SimpleVariable_ReplacesValue()
    {
        var template = """{"message": "Hello ${name}!"}""";
        var context = """{"name": "World"}""";
        var result = JsonE.Render(template, context);
        using var doc = JsonDocument.Parse(result);
        Assert.Equal("Hello World!", doc.RootElement.GetProperty("message").GetString());
    }

    [Fact]
    public void StringInterpolation_Expression_EvaluatesAndReplaces()
    {
        var template = """{"result": "Sum is ${x + y}"}""";
        var context = """{"x": 5, "y": 3}""";
        var result = JsonE.Render(template, context);
        using var doc = JsonDocument.Parse(result);
        Assert.Equal("Sum is 8", doc.RootElement.GetProperty("result").GetString());
    }

    [Fact]
    public void StringInterpolation_MultipleReplacements_AllReplaced()
    {
        var template = """{"greeting": "${greeting}, ${name}!"}""";
        var context = """{"greeting": "Hello", "name": "World"}""";
        var result = JsonE.Render(template, context);
        using var doc = JsonDocument.Parse(result);
        Assert.Equal("Hello, World!", doc.RootElement.GetProperty("greeting").GetString());
    }

    [Fact]
    public void StringInterpolation_NestedProperty_AccessesValue()
    {
        var template = """{"message": "User: ${user.name}"}""";
        var context = """{"user": {"name": "Alice"}}""";
        var result = JsonE.Render(template, context);
        using var doc = JsonDocument.Parse(result);
        Assert.Equal("User: Alice", doc.RootElement.GetProperty("message").GetString());
    }
}
