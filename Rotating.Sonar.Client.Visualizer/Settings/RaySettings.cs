namespace Rotating.Sonar.Client.Visualizer;

public sealed class RaySettings
{
    public FloatColor4 Color { get; set; } = ChainColorParser.Instance.ParseOrThrow("#141414", nameof(Color));

    // TODO: add validation to ensure value is within [0..100].
    public float LengthPercent { get; set; } = 100f;

    // Full cone sweep angle in degrees.
    public float SweepAngleDeg { get; set; } = 15f;

    // Number of segments for the sweep cone interpolation.
    public int SweepSegments { get; set; } = 24;

    // Transparency controls for different ray parts (0..1).
    public float ConeCenterAlpha { get; set; } = 0.75f;
    public float ConeEdgeAlpha { get; set; } = 0.05f;
    public float LineAlpha { get; set; } = 0.65f;

    public float LineWidth { get; set; } = 1.5f;
}
