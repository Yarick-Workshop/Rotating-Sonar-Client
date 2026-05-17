namespace Rotating.Sonar.Client.Common.Settings.Sections;

using System.ComponentModel.DataAnnotations;

public sealed class RangeGridSettings
{
    public FloatColor4 LineColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#4D4D4D80", nameof(LineColor));
    public FloatColor4 RangeLabelColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#99999980", nameof(RangeLabelColor));
    public FloatColor4 BearingLabelColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#C7C7C780", nameof(BearingLabelColor));

    [Range(0.01, float.MaxValue, ErrorMessage = "RangeRingStepCm must be positive.")]
    public float RangeRingStepCm { get; set; } = 100f;

    [Range(0.1, float.MaxValue, ErrorMessage = "OuterRingLineWidthPx must be positive.")]
    public float OuterRingLineWidthPx { get; set; } = 2f;

    [Range(1, 360, ErrorMessage = "RadialLineStepDeg must be between 1 and 360.")]
    public int RadialLineStepDeg { get; set; } = 30;

    [Range(1, 360, ErrorMessage = "TickStepDeg must be between 1 and 360.")]
    public int TickStepDeg { get; set; } = 5;

    [Range(1, 360, ErrorMessage = "MajorTickPerSteps must be between 1 and 360.")]
    public int MajorTickPerSteps { get; set; } = 2;

    [Range(0, float.MaxValue, ErrorMessage = "MajorTickLengthPx must be non-negative.")]
    public float MajorTickLengthPx { get; set; } = 12f;

    [Range(0, float.MaxValue, ErrorMessage = "MinorTickLengthPx must be non-negative.")]
    public float MinorTickLengthPx { get; set; } = 7f;

    [Range(1, 360, ErrorMessage = "BearingLabelStepDeg must be between 1 and 360.")]
    public int BearingLabelStepDeg { get; set; } = 30;

    [Range(3, int.MaxValue, ErrorMessage = "GridCircleSegments must be at least 3.")]
    public int GridCircleSegments { get; set; } = 64;
}
