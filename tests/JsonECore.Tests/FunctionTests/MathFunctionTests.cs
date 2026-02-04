using System.Text.Json;
using JsonECore;

namespace JsonECore.Tests.FunctionTests;

public class MathFunctionTests
{
    [Fact]
    public void Min_MultipleNumbers_ReturnsSmallest()
    {
        var result = JsonE.Render("""{"$eval": "min(5, 3, 8, 1)"}""", "{}");
        Assert.Equal("1", result);
    }

    [Fact]
    public void Max_MultipleNumbers_ReturnsLargest()
    {
        var result = JsonE.Render("""{"$eval": "max(5, 3, 8, 1)"}""", "{}");
        Assert.Equal("8", result);
    }

    [Fact]
    public void Sqrt_ValidNumber_ReturnsSquareRoot()
    {
        var result = JsonE.Render("""{"$eval": "sqrt(16)"}""", "{}");
        Assert.Equal("4", result);
    }

    [Fact]
    public void Ceil_DecimalNumber_ReturnsCeiling()
    {
        var result = JsonE.Render("""{"$eval": "ceil(3.2)"}""", "{}");
        Assert.Equal("4", result);
    }

    [Fact]
    public void Floor_DecimalNumber_ReturnsFloor()
    {
        var result = JsonE.Render("""{"$eval": "floor(3.8)"}""", "{}");
        Assert.Equal("3", result);
    }

    [Fact]
    public void Abs_NegativeNumber_ReturnsPositive()
    {
        var result = JsonE.Render("""{"$eval": "abs(-5)"}""", "{}");
        Assert.Equal("5", result);
    }
}
