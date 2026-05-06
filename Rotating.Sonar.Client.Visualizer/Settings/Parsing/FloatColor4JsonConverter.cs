namespace Rotating.Sonar.Client.Visualizer;

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class FloatColor4JsonConverter : JsonConverter<FloatColor4>
{
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

        return new FloatColor4(0f, 0f, 0f, 1f);
    }

    public override FloatColor4 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException($"FloatColor4 must be a JSON string, got {reader.TokenType}.");
        }

        var raw = reader.GetString();
        return ParseOrThrow(raw);
    }

    public override void Write(Utf8JsonWriter writer, FloatColor4 value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(
            $"{FormatByte(value.R)},{FormatByte(value.G)},{FormatByte(value.B)},{FormatByte(value.A)}");
    }

    internal static byte FormatByte(float value)
    {
        return (byte)Math.Clamp((int)Math.Round(value * 255f), 0, 255);
    }

    private static FloatColor4 ParseOrThrow(string? raw)
    {
        if (TryParse(raw, out var parsed))
        {
            return parsed;
        }

        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new JsonException("Color string is null or empty.");
        }

        throw new JsonException($"Invalid color format: '{raw}'.");
    }

    /// <summary>Parses the same color string formats as JSON deserialization; used by <see cref="FloatColor4TypeConverter"/> for configuration binding.</summary>
    public static bool TryParse(string? raw, out FloatColor4 color)
    {
        color = new FloatColor4(0f, 0f, 0f, 1f);
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

        if (!byte.TryParse(parts[0], out var rr) || !byte.TryParse(parts[1], out var gg) || !byte.TryParse(parts[2], out var bb))
        {
            return false;
        }

        byte aa = 255;
        if (parts.Length == 4 && !byte.TryParse(parts[3], out aa))
        {
            return false;
        }

        color = new FloatColor4(rr / 255f, gg / 255f, bb / 255f, aa / 255f);
        return true;
    }
}
