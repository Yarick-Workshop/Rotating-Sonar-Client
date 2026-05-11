namespace Rotating.Sonar.Client.Common.Settings;

public sealed class SweepSettings
{
    public FloatColor4 SweepColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#141414", nameof(SweepColor));

    // TODO: add validation to ensure value is within [0..100].
    public float LengthPercentOfRadius { get; set; } = 100f;

    // Full cone sweep angle in degrees.
    public float SweepAngleDeg { get; set; } = 15f;

    // Number of segments for the sweep cone interpolation.
    public int SweepSegmentCount { get; set; } = 24;

    // Transparency controls for different sweep parts (0..1).
    public float ConeCenterAlpha { get; set; } = 0.75f;
    public float ConeEdgeAlpha { get; set; } = 0.05f;
    public float LineAlpha { get; set; } = 0.65f;

    public float LineWidthPx { get; set; } = 1.5f;
}
