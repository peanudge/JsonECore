using System.Text.Json;
using JsonECore;

namespace JsonECore.Tests.OperatorTests;

public class IfOperatorTests
{
    [Fact]
    public void If_TrueCondition_ReturnsThenValue()
    {
        var template = """{"$if": "true", "then": "yes", "else": "no"}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        Assert.Equal("\"yes\"", result);
    }

    [Fact]
    public void If_FalseCondition_ReturnsElseValue()
    {
        var template = """{"$if": "false", "then": "yes", "else": "no"}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        Assert.Equal("\"no\"", result);
    }

    [Fact]
    public void If_WithContextVariable_EvaluatesCondition()
    {
        var template = """{"$if": "x > 5", "then": "big", "else": "small"}""";
        var context = """{"x": 10}""";
        var result = JsonE.Render(template, context);
        Assert.Equal("\"big\"", result);
    }

    [Fact]
    public void If_ComplexThenValue_ReturnsRenderedValue()
    {
        var template = """{"$if": "enabled", "then": {"status": "active"}, "else": {"status": "inactive"}}""";
        var context = """{"enabled": true}""";
        var result = JsonE.Render(template, context);
        using var doc = JsonDocument.Parse(result);
        Assert.Equal("active", doc.RootElement.GetProperty("status").GetString());
    }
}
