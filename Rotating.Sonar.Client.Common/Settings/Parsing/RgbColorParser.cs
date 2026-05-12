namespace Rotating.Sonar.Client.Common.Settings;

using System.Globalization;

internal sealed class RgbColorParser : IColorParser
{
    public bool TryParse(string raw, out FloatColor4 color)
    {
        color = default!;
        if (raw.StartsWith('#'))
        {
            return false;
        }

        var parts = raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3 && parts.Length != 4)
        {
            return false;
        }

        var r = ParseByteComponent(parts[0], "R");
        var g = ParseByteComponent(parts[1], "G");
        var b = ParseByteComponent(parts[2], "B");

        byte a = parts.Length == 4 ? ParseByteComponent(parts[3], "A") : (byte)255;

        color = new FloatColor4(r / 255f, g / 255f, b / 255f, a / 255f);
        return true;
    }

    private static byte ParseByteComponent(string raw, string name)
    {
        if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new FormatException($"'{name}' component is not a valid integer: '{raw}'.");
        }

        if (parsed < 0 || parsed > 255)
        {
            throw new FormatException($"'{name}' component is out of range ({parsed}). Allowed range is 0..255.");
        }

        return (byte)parsed;
    }
}
