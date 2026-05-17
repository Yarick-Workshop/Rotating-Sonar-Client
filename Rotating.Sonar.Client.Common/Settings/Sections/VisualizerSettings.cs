namespace Rotating.Sonar.Client.Common.Settings.Sections;

public sealed class VisualizerSettings
{
    public FloatColor4 BackgroundColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#000000", nameof(BackgroundColor));
    public FloatColor4 OverlayTextColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#FFFFFF", nameof(OverlayTextColor));
    public WindowSettings Window { get; set; } = new();
    public ScanAreaSettings ScanArea { get; set; } = new();
    public FpsSettings Fps { get; set; } = new();
    public ZoomSettings Zoom { get; set; } = new();
    public TextRenderingSettings TextRender { get; set; } = new();
    public ScanPointSettings ScanPoints { get; set; } = new();
    public SweepSettings Sweep { get; set; } = new();
    public RangeGridSettings RangeGrid { get; set; } = new();
}
