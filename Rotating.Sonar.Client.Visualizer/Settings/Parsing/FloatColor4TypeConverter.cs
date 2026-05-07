namespace Rotating.Sonar.Client.Visualizer;

using System.ComponentModel;
using System.Globalization;

public sealed class FloatColor4TypeConverter : TypeConverter
{
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
            if (FloatColor4JsonConverter.TryParse(s, out var color))
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
            return $"{FloatColor4JsonConverter.FormatByte(c.R)},{FloatColor4JsonConverter.FormatByte(c.G)},{FloatColor4JsonConverter.FormatByte(c.B)},{FloatColor4JsonConverter.FormatByte(c.A)}";
        }

        throw this.GetConvertToException(value, destinationType);
    }
}
