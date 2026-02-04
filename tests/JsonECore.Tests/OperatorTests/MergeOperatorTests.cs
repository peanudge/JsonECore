using System.Text.Json;
using JsonECore;

namespace JsonECore.Tests.OperatorTests;

public class MergeOperatorTests
{
    [Fact]
    public void Merge_TwoObjects_MergesProperties()
    {
        var template = """{"$merge": [{"a": 1}, {"b": 2}]}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        using var doc = JsonDocument.Parse(result);
        Assert.Equal(1, doc.RootElement.GetProperty("a").GetInt32());
        Assert.Equal(2, doc.RootElement.GetProperty("b").GetInt32());
    }

    [Fact]
    public void Merge_OverlappingKeys_LaterWins()
    {
        var template = """{"$merge": [{"a": 1}, {"a": 2}]}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        using var doc = JsonDocument.Parse(result);
        Assert.Equal(2, doc.RootElement.GetProperty("a").GetInt32());
    }

    [Fact]
    public void MergeDeep_NestedObjects_MergesRecursively()
    {
        var template = """{"$mergeDeep": [{"a": {"b": 1}}, {"a": {"c": 2}}]}""";
        var context = "{}";
        var result = JsonE.Render(template, context);
        using var doc = JsonDocument.Parse(result);
        Assert.Equal(1, doc.RootElement.GetProperty("a").GetProperty("b").GetInt32());
        Assert.Equal(2, doc.RootElement.GetProperty("a").GetProperty("c").GetInt32());
    }
}
