namespace Rotating.Sonar.Client.Visualizer;

public sealed class PointsSettings
{
    public float PointSizePx { get; set; } = 12f;
    public FloatColor4 EchoColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#FF3333", nameof(EchoColor));
    public FloatColor4 CenterPointColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#FFFFFF", nameof(CenterPointColor));
    public FloatColor4 OverflowArrowColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#F2261F", nameof(OverflowArrowColor));
}
