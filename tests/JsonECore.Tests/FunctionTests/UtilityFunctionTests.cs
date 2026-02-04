using System.Text.Json;
using JsonECore;

namespace JsonECore.Tests.FunctionTests;

public class UtilityFunctionTests
{
    [Fact]
    public void Range_SingleArg_ReturnsRangeFromZero()
    {
        var result = JsonE.Render("""{"$eval": "range(5)"}""", "{}");
        Assert.Equal("[0,1,2,3,4]", result);
    }

    [Fact]
    public void Range_TwoArgs_ReturnsRangeFromStart()
    {
        var result = JsonE.Render("""{"$eval": "range(2, 5)"}""", "{}");
        Assert.Equal("[2,3,4]", result);
    }

    [Fact]
    public void Range_ThreeArgs_ReturnsRangeWithStep()
    {
        var result = JsonE.Render("""{"$eval": "range(0, 10, 2)"}""", "{}");
        Assert.Equal("[0,2,4,6,8]", result);
    }

    [Fact]
    public void Range_NegativeStep_ReturnsDecreasingRange()
    {
        var result = JsonE.Render("""{"$eval": "range(5, 0, -1)"}""", "{}");
        Assert.Equal("[5,4,3,2,1]", result);
    }

    [Fact]
    public void Defined_ExistingVariable_ReturnsTrue()
    {
        var result = JsonE.Render("""{"$eval": "defined('x')"}""", """{"x": 10}""");
        Assert.Equal("true", result);
    }

    [Fact]
    public void Defined_NonExistingVariable_ReturnsFalse()
    {
        var result = JsonE.Render("""{"$eval": "defined('y')"}""", """{"x": 10}""");
        Assert.Equal("false", result);
    }
}
