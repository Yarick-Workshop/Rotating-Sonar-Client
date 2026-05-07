namespace Rotating.Sonar.Client.Visualizer;

public sealed class PointColorsSettings
{
    public FloatColor4 EchoColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#FF3333", nameof(EchoColor));
    public FloatColor4 CenterColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#FFFFFF", nameof(CenterColor));
    public FloatColor4 OverflowArrowColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#F2261F", nameof(OverflowArrowColor));
}
