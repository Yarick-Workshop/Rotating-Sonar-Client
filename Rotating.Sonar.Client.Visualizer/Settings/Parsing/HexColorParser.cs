namespace Rotating.Sonar.Client.Visualizer;

using System.Globalization;

internal sealed class HexColorParser : IColorParser
{
    public bool TryParse(string raw, out FloatColor4 color)
    {
        color = default!;
        if (!raw.StartsWith('#'))
        {
            return false;
        }

        var hex = raw[1..];
        if (hex.Length != 6 && hex.Length != 8)
        {
            return false;
        }

        if (hex.Length == 6 && uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgb))
        {
            var r = (byte)((rgb >> 16) & 0xFF);
            var g = (byte)((rgb >> 8) & 0xFF);
            var b = (byte)(rgb & 0xFF);
            color = new FloatColor4(r / 255f, g / 255f, b / 255f, 1f);
            return true;
        }

        if (hex.Length == 8 && uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rgba))
        {
            var r = (byte)((rgba >> 24) & 0xFF);
            var g = (byte)((rgba >> 16) & 0xFF);
            var b = (byte)((rgba >> 8) & 0xFF);
            var a = (byte)(rgba & 0xFF);
            color = new FloatColor4(r / 255f, g / 255f, b / 255f, a / 255f);
            return true;
        }

        return false;
    }
}
