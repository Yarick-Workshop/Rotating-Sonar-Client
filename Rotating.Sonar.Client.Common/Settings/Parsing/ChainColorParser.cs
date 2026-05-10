namespace Rotating.Sonar.Client.Common.Settings;

using System.Reflection;

internal sealed class ChainColorParser : IColorParser
{
    public static ChainColorParser Instance { get; } = new();

    private readonly IColorParser[] parsers;

    private ChainColorParser()
    {
        this.parsers = this.CreateParsers();
    }

    public FloatColor4 ParseOrThrow(string raw, string colorName)
    {
        if (this.TryParse(raw, out var color))
        {
            return color;
        }

        throw new FormatException($"Could not parse color '{colorName}' from value '{raw}'.");
    }

    public bool TryParse(string raw, out FloatColor4 color)
    {
        foreach (var parser in this.parsers)
        {
            if (parser.TryParse(raw, out color))
            {
                return true;
            }
        }

        color = default!;
        return false;
    }

    private IColorParser[] CreateParsers()
    {
        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IColorParser).IsAssignableFrom(t))
            .Where(t => t is { IsClass: true, IsAbstract: false })
            .Where(t => t != this.GetType())
            .OrderBy(t => t.Name)
            .Select(t => Activator.CreateInstance(t))
            .OfType<IColorParser>()
            .ToArray();
    }
}
