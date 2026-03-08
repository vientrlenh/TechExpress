using System.Reflection;
using System.Text.Json;
using Anthropic.Models.Messages;

var asm = typeof(Tool).Assembly;
var types = new[] { "Anthropic.Models.Messages.InputSchema", "Anthropic.Models.Messages.Tool", 
                    "Anthropic.Models.Messages.ToolUseBlock", "Anthropic.Models.Messages.ToolUnion",
                    "Anthropic.Models.Messages.ToolResultBlockParam", "Anthropic.Models.Messages.ContentBlockParam",
                    "Anthropic.Models.Messages.MessageParamContent", "Anthropic.Models.Messages.StopReason",
                    "Anthropic.Models.Messages.ToolResultBlockParamContent", "Anthropic.Models.Messages.TextBlockParam",
                    "Anthropic.Models.Messages.ToolUseBlockParam" };
foreach (var typeName in types) {
    var t = asm.GetType(typeName);
    if (t == null) { Console.WriteLine($"{typeName}: NOT FOUND"); continue; }
    Console.WriteLine($"\n=== {typeName} ===");
    if (t.IsEnum) {
        foreach (var val in Enum.GetNames(t)) Console.WriteLine($"  {val}");
    } else {
        foreach (var p in t.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            Console.WriteLine($"  prop: {p.PropertyType.FullName} {p.Name}");
        foreach (var c in t.GetConstructors())
            Console.WriteLine($"  ctor({string.Join(", ", Array.ConvertAll(c.GetParameters(), p => p.ParameterType.Name + " " + p.Name))})");
    }
}
