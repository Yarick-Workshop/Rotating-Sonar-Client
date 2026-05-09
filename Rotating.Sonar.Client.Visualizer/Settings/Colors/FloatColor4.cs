namespace Rotating.Sonar.Client.Visualizer;

using System.ComponentModel;

[TypeConverter(typeof(FloatColor4TypeConverter))]
public sealed class FloatColor4(float r, float g, float b, float a)
{
    public float R { get; } = r;
    public float G { get; } = g;
    public float B { get; } = b;
    public float A { get; } = a;
}
