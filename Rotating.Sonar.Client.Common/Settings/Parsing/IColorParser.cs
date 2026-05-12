namespace Rotating.Sonar.Client.Common.Settings;

internal interface IColorParser
{
    bool TryParse(string raw, out FloatColor4 color);
}
