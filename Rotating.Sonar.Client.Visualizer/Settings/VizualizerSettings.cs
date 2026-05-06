namespace Rotating.Sonar.Client.Visualizer;

public sealed class VizualizerSettings
{
    public FloatColor4 BackgroundColor { get; set; } = new(0f, 0f, 0f, 1f);
    public FloatColor4 UiTextColor { get; set; } = new(1f, 1f, 1f, 1f);
    public PointColorsSettings Points { get; set; } = new();
    public GridColorsSettings Grid { get; set; } = new();
}
