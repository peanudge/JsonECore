using System.Text.Json;
using JsonECore;

namespace JsonECore.Tests.OperatorTests;

public class MapOperatorTests
{
    [Fact]
    public void Map_SimpleArray_TransformsElements()
    {
        var template = """{"$map": [1, 2, 3], "each(x)": {"$eval": "x * 2"}}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        Assert.Equal("[2,4,6]", result);
    }

    [Fact]
    public void Map_WithIndex_ProvidesIndex()
    {
        var template = """{"$map": ["a", "b", "c"], "each(x, i)": {"$eval": "i"}}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        Assert.Equal("[0,1,2]", result);
    }

    [Fact]
    public void Map_ContextArray_TransformsElements()
    {
        var template = """{"$map": {"$eval": "items"}, "each(x)": {"$eval": "x + 1"}}""";
        var context = """{"items": [1, 2, 3]}""";
        var result = JsonE.Render(template, context);
        Assert.Equal("[2,3,4]", result);
    }

    [Fact]
    public void Map_ObjectTemplate_TransformsToObjects()
    {
        var template = """{"$map": [1, 2], "each(x)": {"value": {"$eval": "x"}}}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        using var doc = JsonDocument.Parse(result);
        var arr = doc.RootElement.EnumerateArray().ToList();
        Assert.Equal(2, arr.Count);
        Assert.Equal(1, arr[0].GetProperty("value").GetInt32());
        Assert.Equal(2, arr[1].GetProperty("value").GetInt32());
    }
}
