using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using JsonECore;

BenchmarkRunner.Run<JsonEBenchmarks>();

[MemoryDiagnoser]
public class JsonEBenchmarks
{
    private string _simpleTemplate = null!;
    private string _simpleContext = null!;
    private string _ifTemplate = null!;
    private string _ifContext = null!;
    private string _mapTemplate = null!;
    private string _mapContext = null!;
    private string _complexTemplate = null!;
    private string _complexContext = null!;
    private string _interpolationTemplate = null!;
    private string _interpolationContext = null!;

    [GlobalSetup]
    public void Setup()
    {
        _simpleTemplate = """{"$eval": "1 + 2 * 3"}""";
        _simpleContext = "{}";

        _ifTemplate = """{"$if": "x > 5", "then": "big", "else": "small"}""";
        _ifContext = """{"x": 10}""";

        _mapTemplate = """{"$map": [1, 2, 3, 4, 5], "each(x)": {"$eval": "x * 2"}}""";
        _mapContext = "{}";

        _complexTemplate = """
        {
            "users": {
                "$map": {"$eval": "items"},
                "each(item)": {
                    "name": {"$eval": "uppercase(item.name)"},
                    "active": {"$if": "item.score > 50", "then": true, "else": false}
                }
            },
            "total": {"$eval": "len(items)"}
        }
        """;
        _complexContext = """
        {
            "items": [
                {"name": "alice", "score": 75},
                {"name": "bob", "score": 40},
                {"name": "charlie", "score": 90}
            ]
        }
        """;

        _interpolationTemplate = """{"message": "Hello ${name}! Your score is ${score}."}""";
        _interpolationContext = """{"name": "World", "score": 100}""";
    }

    [Benchmark]
    public string SimpleExpression()
    {
        return JsonE.Render(_simpleTemplate, _simpleContext);
    }

    [Benchmark]
    public string ConditionalIf()
    {
        return JsonE.Render(_ifTemplate, _ifContext);
    }

    [Benchmark]
    public string MapArray()
    {
        return JsonE.Render(_mapTemplate, _mapContext);
    }

    [Benchmark]
    public string ComplexTemplate()
    {
        return JsonE.Render(_complexTemplate, _complexContext);
    }

    [Benchmark]
    public string StringInterpolation()
    {
        return JsonE.Render(_interpolationTemplate, _interpolationContext);
    }
}
