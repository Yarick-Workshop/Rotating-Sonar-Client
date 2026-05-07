namespace Rotating.Sonar.Client.Visualizer;

internal interface IColorParser
{
    bool TryParse(string raw, out FloatColor4 color);
}
