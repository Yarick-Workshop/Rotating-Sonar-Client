namespace Rotating.Sonar.Client.Visualizer;

using System.ComponentModel;
using System.Globalization;

public sealed class FloatColor4TypeConverter : TypeConverter
{
    private static readonly FloatColor4 DefaultColor = new(0f, 0f, 0f, 1f);

    public static FloatColor4 ParseOrDefault(string? raw, string fallbackRaw)
    {
        if (TryParse(raw, out var parsed))
        {
            return parsed;
        }

        if (TryParse(fallbackRaw, out parsed))
        {
            return parsed;
        }

        return DefaultColor;
    }

    public static bool TryParse(string? raw, out FloatColor4 color)
    {
        color = DefaultColor;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        var s = raw.Trim();
        if (s.StartsWith('#'))
        {
            var hex = s[1..];
            if (hex.Length == 3)
            {
                hex = string.Concat(hex[0], hex[0], hex[1], hex[1], hex[2], hex[2]);
            }

            if (hex.Length == 4)
            {
                hex = string.Concat(hex[0], hex[0], hex[1], hex[1], hex[2], hex[2], hex[3], hex[3]);
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

        var parts = s.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3 && parts.Length != 4)
        {
            return false;
        }

        if (!byte.TryParse(parts[0], NumberStyles.Integer, CultureInfo.InvariantCulture, out var rr)
            || !byte.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var gg)
            || !byte.TryParse(parts[2], NumberStyles.Integer, CultureInfo.InvariantCulture, out var bb))
        {
            return false;
        }

        byte aa = 255;
        if (parts.Length == 4
            && !byte.TryParse(parts[3], NumberStyles.Integer, CultureInfo.InvariantCulture, out aa))
        {
            return false;
        }

        color = new FloatColor4(rr / 255f, gg / 255f, bb / 255f, aa / 255f);
        return true;
    }

    internal static byte FormatByte(float value)
    {
        return (byte)Math.Clamp((int)Math.Round(value * 255f), 0, 255);
    }

    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
    }

    public override object? ConvertFrom(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object value)
    {
        if (value is string s)
        {
            if (TryParse(s, out var color))
            {
                return color;
            }

            throw new FormatException($"Invalid FloatColor4 format: '{s}'.");
        }

        throw this.GetConvertFromException(value);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string) || base.CanConvertTo(context, destinationType);
    }

    public override object? ConvertTo(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object? value,
        Type destinationType)
    {
        ArgumentNullException.ThrowIfNull(destinationType);

        if (destinationType == typeof(string) && value is FloatColor4 c)
        {
            return $"{FormatByte(c.R)},{FormatByte(c.G)},{FormatByte(c.B)},{FormatByte(c.A)}";
        }

        if (destinationType == typeof(string) && value is null)
        {
            return string.Empty;
        }

        throw this.GetConvertToException(value, destinationType);
    }
}
