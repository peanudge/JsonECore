using System.Text.Json;
using JsonECore;

namespace JsonECore.Tests.OperatorTests;

public class EvalOperatorTests
{
    [Fact]
    public void Eval_SimpleExpression_ReturnsResult()
    {
        var template = """{"$eval": "1 + 2"}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        Assert.Equal("3", result);
    }

    [Fact]
    public void Eval_WithContext_ReturnsResult()
    {
        var template = """{"$eval": "x + y"}""";
        var context = """{"x": 5, "y": 3}""";
        var result = JsonE.Render(template, context);
        Assert.Equal("8", result);
    }

    [Fact]
    public void Eval_StringConcatenation_ReturnsResult()
    {
        var template = """{"$eval": "'hello' + ' ' + 'world'"}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        Assert.Equal("\"hello world\"", result);
    }

    [Fact]
    public void Eval_Comparison_ReturnsBoolean()
    {
        var template = """{"$eval": "5 > 3"}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        Assert.Equal("true", result);
    }

    [Fact]
    public void Eval_LogicalOperators_ReturnsBoolean()
    {
        var template = """{"$eval": "true && false"}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        Assert.Equal("false", result);
    }

    [Fact]
    public void Eval_PowerOperator_ReturnsResult()
    {
        var template = """{"$eval": "2 ** 3"}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        Assert.Equal("8", result);
    }
}
