namespace Rotating.Sonar.Client.Visualizer;

public sealed class PointColorsSettings
{
    public FloatColor4 EchoColor { get; set; } = new(1f, 0.2f, 0.2f, 1f);
    public FloatColor4 CenterColor { get; set; } = new(1f, 1f, 1f, 1f);
    public FloatColor4 OverflowArrowColor { get; set; } = new(0.949f, 0.149f, 0.122f, 1f);
}
