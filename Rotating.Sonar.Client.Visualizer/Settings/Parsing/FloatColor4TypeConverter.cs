namespace Rotating.Sonar.Client.Visualizer;

using System.ComponentModel;
using System.Globalization;

public sealed class FloatColor4TypeConverter : TypeConverter
{
    private static readonly IColorParser Parser = new ChainColorParser();

    public static FloatColor4 Parse(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new FormatException("FloatColor4 cannot be null or empty.");
        }

        var s = raw.Trim();
        if (Parser.TryParse(s, out var color))
        {
            return color;
        }

        throw new FormatException(
            $"Invalid FloatColor4 format: '{raw}'. Use '#RRGGBB', '#RRGGBBAA', 'R,G,B', or 'R,G,B,A'.");
    }

    internal static byte FormatByte(float value)
    {
        return (byte)Math.Clamp((int)Math.Round(value * 255f), 0, 255);
    }

    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
    {
        return sourceType == typeof(string)/* TODO, check if this is needed || base.CanConvertFrom(context, sourceType)*/;
    }

    public override object? ConvertFrom(
        ITypeDescriptorContext? context,
        CultureInfo? culture,
        object value)
    {
        if (value is string s)
        {
            return Parse(s);
        }

        throw this.GetConvertFromException(value);
    }

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
    {
        return destinationType == typeof(string)/* TODO, is it needed? || base.CanConvertTo(context, destinationType)*/;
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

        throw this.GetConvertToException(value, destinationType);
    }
}
