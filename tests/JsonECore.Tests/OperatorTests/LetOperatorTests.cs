using System.Text.Json;
using JsonECore;

namespace JsonECore.Tests.OperatorTests;

public class LetOperatorTests
{
    [Fact]
    public void Let_SingleBinding_CreatesVariable()
    {
        var template = """{"$let": {"x": 10}, "in": {"$eval": "x + 5"}}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        Assert.Equal("15", result);
    }

    [Fact]
    public void Let_MultipleBindings_CreatesVariables()
    {
        var template = """{"$let": {"x": 10, "y": 20}, "in": {"$eval": "x + y"}}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        Assert.Equal("30", result);
    }

    [Fact]
    public void Let_OverridesContextVariable()
    {
        var template = """{"$let": {"x": 100}, "in": {"$eval": "x"}}""";
        var context = """{"x": 10}""";
        var result = JsonE.Render(template, context);
        Assert.Equal("100", result);
    }

    [Fact]
    public void Let_NestedLet_InnerScopeTakesPrecedence()
    {
        var template = """{"$let": {"x": 1}, "in": {"$let": {"x": 2}, "in": {"$eval": "x"}}}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        Assert.Equal("2", result);
    }
}
