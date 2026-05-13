namespace Rotating.Sonar.Client.Common.Settings;

public sealed class RangeGridSettings
{
    public FloatColor4 LineColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#4D4D4D80", nameof(LineColor));
    public FloatColor4 RangeLabelColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#99999980", nameof(RangeLabelColor));
    public FloatColor4 BearingLabelColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#C7C7C780", nameof(BearingLabelColor));
    public float RangeRingStepCm { get; set; } = 100f;
    public float OuterRingLineWidthPx { get; set; } = 2f;
    public int RadialLineStepDeg { get; set; } = 30;
    public int TickStepDeg { get; set; } = 5;
    public int MajorTickStepDeg { get; set; } = 10;
    public float MajorTickLengthPx { get; set; } = 12f;
    public float MinorTickLengthPx { get; set; } = 7f;
    public int BearingLabelStepDeg { get; set; } = 30;
    public int GridCircleSegments { get; set; } = 64;
}
