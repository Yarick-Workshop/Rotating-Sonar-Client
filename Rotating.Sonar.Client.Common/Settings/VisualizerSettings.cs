namespace Rotating.Sonar.Client.Common.Settings;

public sealed class VisualizerSettings
{
    public FloatColor4 BackgroundColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#000000", nameof(BackgroundColor));
    public FloatColor4 OverlayTextColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#FFFFFF", nameof(OverlayTextColor));
    public ScanPointSettings ScanPoints { get; set; } = new();
    public SweepSettings Sweep { get; set; } = new();
    public RangeGridSettings RangeGrid { get; set; } = new();
}
