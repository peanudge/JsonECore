using System.Text.Json;
using JsonECore;

namespace JsonECore.Tests.ExpressionTests;

public class ExpressionParserTests
{
    [Theory]
    [InlineData("""{"$eval": "1 + 2"}""", "3")]
    [InlineData("""{"$eval": "10 - 3"}""", "7")]
    [InlineData("""{"$eval": "4 * 5"}""", "20")]
    [InlineData("""{"$eval": "15 / 3"}""", "5")]
    [InlineData("""{"$eval": "10 % 3"}""", "1")]
    [InlineData("""{"$eval": "2 ** 4"}""", "16")]
    public void ArithmeticOperators_ReturnCorrectResults(string template, string expected)
    {
        var result = JsonE.Render(template, "{}");
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("""{"$eval": "5 == 5"}""", "true")]
    [InlineData("""{"$eval": "5 != 3"}""", "true")]
    [InlineData("""{"$eval": "5 < 10"}""", "true")]
    [InlineData("""{"$eval": "5 <= 5"}""", "true")]
    [InlineData("""{"$eval": "10 > 5"}""", "true")]
    [InlineData("""{"$eval": "5 >= 5"}""", "true")]
    public void ComparisonOperators_ReturnCorrectResults(string template, string expected)
    {
        var result = JsonE.Render(template, "{}");
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("""{"$eval": "true && true"}""", "true")]
    [InlineData("""{"$eval": "true && false"}""", "false")]
    [InlineData("""{"$eval": "false || true"}""", "true")]
    [InlineData("""{"$eval": "false || false"}""", "false")]
    [InlineData("""{"$eval": "!true"}""", "false")]
    [InlineData("""{"$eval": "!false"}""", "true")]
    public void LogicalOperators_ReturnCorrectResults(string template, string expected)
    {
        var result = JsonE.Render(template, "{}");
        Assert.Equal(expected, result);
    }

    [Fact]
    public void InOperator_ArrayContains_ReturnsTrue()
    {
        var result = JsonE.Render("""{"$eval": "2 in [1, 2, 3]"}""", "{}");
        Assert.Equal("true", result);
    }

    [Fact]
    public void InOperator_ArrayNotContains_ReturnsFalse()
    {
        var result = JsonE.Render("""{"$eval": "5 in [1, 2, 3]"}""", "{}");
        Assert.Equal("false", result);
    }

    [Fact]
    public void InOperator_StringContains_ReturnsTrue()
    {
        var result = JsonE.Render("""{"$eval": "'el' in 'hello'"}""", "{}");
        Assert.Equal("true", result);
    }

    [Fact]
    public void ConditionalExpression_TrueCondition_ReturnsTrueValue()
    {
        var result = JsonE.Render("""{"$eval": "true ? 'yes' : 'no'"}""", "{}");
        Assert.Equal("\"yes\"", result);
    }

    [Fact]
    public void ConditionalExpression_FalseCondition_ReturnsFalseValue()
    {
        var result = JsonE.Render("""{"$eval": "false ? 'yes' : 'no'"}""", "{}");
        Assert.Equal("\"no\"", result);
    }

    [Fact]
    public void ArrayAccess_ValidIndex_ReturnsElement()
    {
        var result = JsonE.Render("""{"$eval": "arr[1]"}""", """{"arr": [10, 20, 30]}""");
        Assert.Equal("20", result);
    }

    [Fact]
    public void ArrayAccess_NegativeIndex_ReturnsFromEnd()
    {
        var result = JsonE.Render("""{"$eval": "arr[-1]"}""", """{"arr": [10, 20, 30]}""");
        Assert.Equal("30", result);
    }

    [Fact]
    public void ArraySlice_StartEnd_ReturnsSubarray()
    {
        var result = JsonE.Render("""{"$eval": "arr[1:3]"}""", """{"arr": [10, 20, 30, 40]}""");
        Assert.Equal("[20,30]", result);
    }

    [Fact]
    public void PropertyAccess_ValidProperty_ReturnsValue()
    {
        var result = JsonE.Render("""{"$eval": "obj.name"}""", """{"obj": {"name": "test"}}""");
        Assert.Equal("\"test\"", result);
    }
}
