namespace Rotating.Sonar.Client.Visualizer;

using System.ComponentModel;
using System.Text.Json.Serialization;

// TODO, do we need both?
[TypeConverter(typeof(FloatColor4TypeConverter))]
[JsonConverter(typeof(FloatColor4JsonConverter))]
public sealed class FloatColor4(float r, float g, float b, float a)
{
    public float R { get; } = r;
    public float G { get; } = g;
    public float B { get; } = b;
    public float A { get; } = a;
}
