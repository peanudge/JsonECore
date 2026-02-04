using System.Text.Json;
using JsonECore;

namespace JsonECore.Tests.OperatorTests;

public class MoreOperatorTests
{
    [Fact]
    public void Switch_MatchingCase_ReturnsValue()
    {
        var template = """{"$switch": {"x == 1": "one", "x == 2": "two", "$default": "other"}}""";
        var context = """{"x": 2}""";
        var result = JsonE.Render(template, context);
        Assert.Equal("\"two\"", result);
    }

    [Fact]
    public void Switch_NoMatch_ReturnsDefault()
    {
        var template = """{"$switch": {"x == 1": "one", "$default": "other"}}""";
        var context = """{"x": 99}""";
        var result = JsonE.Render(template, context);
        Assert.Equal("\"other\"", result);
    }

    [Fact]
    public void Match_MultipleMatches_ReturnsAllMatches()
    {
        var template = """{"$match": {"x > 0": "positive", "x < 10": "small", "x == 5": "five"}}""";
        var context = """{"x": 5}""";
        var result = JsonE.Render(template, context);
        using var doc = JsonDocument.Parse(result);
        var arr = doc.RootElement.EnumerateArray().Select(x => x.GetString()).ToList();
        Assert.Contains("positive", arr);
        Assert.Contains("small", arr);
        Assert.Contains("five", arr);
    }

    [Fact]
    public void Reduce_Sum_ReturnsTotal()
    {
        var template = """{"$reduce": [1, 2, 3, 4, 5], "each(acc, x)": {"$eval": "acc + x"}, "initial": 0}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        Assert.Equal("15", result);
    }

    [Fact]
    public void Reduce_WithIndex_UsesIndex()
    {
        var template = """{"$reduce": ["a", "b", "c"], "each(acc, x, i)": {"$eval": "acc + i"}, "initial": 0}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        Assert.Equal("3", result); // 0 + 1 + 2 = 3
    }

    [Fact]
    public void Find_MatchingElement_ReturnsFirst()
    {
        var template = """{"$find": [1, 2, 3, 4, 5], "each(x)": "x > 3"}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        Assert.Equal("4", result);
    }

    [Fact]
    public void Find_NoMatch_ReturnsNull()
    {
        var template = """{"$find": [1, 2, 3], "each(x)": "x > 10"}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        Assert.Equal("null", result);
    }

    [Fact]
    public void Flatten_NestedArrays_FlattensOneLevel()
    {
        var template = """{"$flatten": [[1, 2], [3, 4], [5]]}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        Assert.Equal("[1,2,3,4,5]", result);
    }

    [Fact]
    public void FlattenDeep_DeeplyNested_FlattensCompletely()
    {
        var template = """{"$flattenDeep": [[[1]], [[2, [3]]], [4]]}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        Assert.Equal("[1,2,3,4]", result);
    }

    [Fact]
    public void Reverse_Array_ReversesOrder()
    {
        var template = """{"$reverse": [1, 2, 3, 4, 5]}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        Assert.Equal("[5,4,3,2,1]", result);
    }

    [Fact]
    public void Reverse_String_ReversesChars()
    {
        var template = """{"$reverse": "hello"}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        Assert.Equal("\"olleh\"", result);
    }

    [Fact]
    public void Sort_Numbers_SortsAscending()
    {
        var template = """{"$sort": [5, 2, 8, 1, 9]}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        Assert.Equal("[1,2,5,8,9]", result);
    }

    [Fact]
    public void Sort_WithBy_SortsByKey()
    {
        var template = """{"$sort": [{"name": "z"}, {"name": "a"}, {"name": "m"}], "by(x)": "x.name"}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        using var doc = JsonDocument.Parse(result);
        var names = doc.RootElement.EnumerateArray()
            .Select(x => x.GetProperty("name").GetString())
            .ToList();
        Assert.Equal(new[] { "a", "m", "z" }, names);
    }

    [Fact]
    public void Json_Object_ReturnsJsonString()
    {
        var template = """{"$json": {"a": 1, "b": 2}}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        // Result should be a string containing the JSON
        using var doc = JsonDocument.Parse(result);
        Assert.Equal(JsonValueKind.String, doc.RootElement.ValueKind);
    }

    [Fact]
    public void DeleteMarker_IfWithoutElse_RemovesKeyFromObject()
    {
        var template = """{"keep": "yes", "remove": {"$if": "false", "then": "value"}}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        using var doc = JsonDocument.Parse(result);
        Assert.True(doc.RootElement.TryGetProperty("keep", out _));
        Assert.False(doc.RootElement.TryGetProperty("remove", out _));
    }

    [Fact]
    public void DeleteMarker_IfWithoutThen_RemovesKeyFromObject()
    {
        var template = """{"keep": "yes", "remove": {"$if": "true"}}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        using var doc = JsonDocument.Parse(result);
        Assert.True(doc.RootElement.TryGetProperty("keep", out _));
        Assert.False(doc.RootElement.TryGetProperty("remove", out _));
    }

    [Fact]
    public void DeleteMarker_InArray_RemovesElement()
    {
        var template = """[1, {"$if": "false", "then": 2}, 3]""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        Assert.Equal("[1,3]", result);
    }
}
