namespace Rotating.Sonar.Client.Common.Settings.Sections;

using System.ComponentModel.DataAnnotations;

public sealed class SweepSettings
{
    public FloatColor4 SweepColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#141414", nameof(SweepColor));

    [Range(0, 100, ErrorMessage = "LengthPercentOfRadius must be between 0 and 100.")]
    public float LengthPercentOfRadius { get; set; } = 100f;

    [Range(0, float.MaxValue, ErrorMessage = "SweepAngleDeg must be non-negative.")]
    public float SweepAngleDeg { get; set; } = 15f;

    [Range(1, int.MaxValue, ErrorMessage = "SweepSegmentCount must be at least 1.")]
    public int SweepSegmentCount { get; set; } = 24;

    [Range(0, 1, ErrorMessage = "ConeCenterAlpha must be between 0 and 1.")]
    public float ConeCenterAlpha { get; set; } = 0.75f;

    [Range(0, 1, ErrorMessage = "ConeEdgeAlpha must be between 0 and 1.")]
    public float ConeEdgeAlpha { get; set; } = 0.05f;

    [Range(0, 1, ErrorMessage = "LineAlpha must be between 0 and 1.")]
    public float LineAlpha { get; set; } = 0.65f;

    [Range(0.01, float.MaxValue, ErrorMessage = "LineWidthPx must be positive.")]
    public float LineWidthPx { get; set; } = 1.5f;
}
