using System.Text.Json;
using JsonECore;

namespace JsonECore.Cli;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length == 0)
        {
            PrintHelp();
            return 0;
        }

        var command = args[0].ToLower();

        switch (command)
        {
            case "eval":
                return RunEval(args.Skip(1).ToArray());
            case "validate":
                return RunValidate(args.Skip(1).ToArray());
            case "help":
            case "--help":
            case "-h":
                PrintHelp();
                return 0;
            case "version":
            case "--version":
            case "-v":
                PrintVersion();
                return 0;
            default:
                Console.Error.WriteLine($"Unknown command: {command}");
                PrintHelp();
                return 1;
        }
    }

    static int RunEval(string[] args)
    {
        string? templatePath = null;
        string? contextPath = null;
        string? templateExpr = null;
        string? contextExpr = null;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-t" or "--template":
                    if (i + 1 < args.Length) templatePath = args[++i];
                    break;
                case "-c" or "--context":
                    if (i + 1 < args.Length) contextPath = args[++i];
                    break;
                case "-e" or "--expr":
                    if (i + 1 < args.Length) templateExpr = args[++i];
                    break;
                case "-x" or "--context-expr":
                    if (i + 1 < args.Length) contextExpr = args[++i];
                    break;
            }
        }

        try
        {
            string template;
            string context;

            // Get template
            if (templateExpr != null)
            {
                template = templateExpr;
            }
            else if (templatePath != null)
            {
                template = File.ReadAllText(templatePath);
            }
            else
            {
                Console.Error.WriteLine("Error: Template required. Use -t <file> or -e '<json>'");
                return 1;
            }

            // Get context
            if (contextExpr != null)
            {
                context = contextExpr;
            }
            else if (contextPath != null)
            {
                context = File.ReadAllText(contextPath);
            }
            else
            {
                context = "{}";
            }

            var result = JsonE.Render(template, context);

            // Pretty print the result
            using var doc = JsonDocument.Parse(result);
            var options = new JsonSerializerOptions { WriteIndented = true };
            Console.WriteLine(JsonSerializer.Serialize(doc.RootElement, options));

            return 0;
        }
        catch (JsonEException ex)
        {
            Console.Error.WriteLine($"Error [{ex.ErrorCode}]: {ex.Message}");
            return 1;
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"JSON Parse Error: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    static int RunValidate(string[] args)
    {
        string? templatePath = null;
        string? templateExpr = null;

        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-t" or "--template":
                    if (i + 1 < args.Length) templatePath = args[++i];
                    break;
                case "-e" or "--expr":
                    if (i + 1 < args.Length) templateExpr = args[++i];
                    break;
            }
        }

        try
        {
            string template;

            if (templateExpr != null)
            {
                template = templateExpr;
            }
            else if (templatePath != null)
            {
                template = File.ReadAllText(templatePath);
            }
            else
            {
                Console.Error.WriteLine("Error: Template required. Use -t <file> or -e '<json>'");
                return 1;
            }

            using var doc = JsonDocument.Parse(template);
            JsonE.Validate(doc.RootElement);

            Console.WriteLine("Template is valid.");
            return 0;
        }
        catch (JsonEException ex)
        {
            Console.Error.WriteLine($"Validation Error [{ex.ErrorCode}]: {ex.Message}");
            return 1;
        }
        catch (JsonException ex)
        {
            Console.Error.WriteLine($"JSON Parse Error: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    static void PrintHelp()
    {
        Console.WriteLine(@"JsonECore CLI - JSON-E Template Engine

Usage:
  jsone <command> [options]

Commands:
  eval        Evaluate a JSON-E template
  validate    Validate a JSON-E template syntax
  help        Show this help message
  version     Show version information

Eval Options:
  -t, --template <file>    Template file path
  -c, --context <file>     Context file path
  -e, --expr <json>        Template as inline JSON
  -x, --context-expr <json> Context as inline JSON

Examples:
  jsone eval -t template.json -c context.json
  jsone eval -e '{""$eval"": ""x + y""}' -x '{""x"": 1, ""y"": 2}'
  jsone validate -t template.json
");
    }

    static void PrintVersion()
    {
        Console.WriteLine("JsonECore CLI v1.0.0");
    }
}
