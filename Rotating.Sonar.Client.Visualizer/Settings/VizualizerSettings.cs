namespace Rotating.Sonar.Client.Visualizer;

public sealed class VizualizerSettings
{
    public FloatColor4 BackgroundColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#000000", nameof(BackgroundColor));
    public FloatColor4 UiTextColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#FFFFFF", nameof(UiTextColor));
    public PointsSettings Points { get; set; } = new();
    public GridColorsSettings Grid { get; set; } = new();
}
