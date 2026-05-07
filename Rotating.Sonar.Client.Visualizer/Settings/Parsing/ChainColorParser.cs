namespace Rotating.Sonar.Client.Visualizer;

using System.Reflection;

internal sealed class ChainColorParser : IColorParser
{
    private readonly IColorParser[] parsers;

    public ChainColorParser()
    {
        this.parsers = this.CreateParsers();
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
