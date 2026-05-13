namespace Rotating.Sonar.Client.Common.Settings;

public sealed class ScanPointSettings
{
    public float PointSizePx { get; set; } = 12f;
    public float LineWidthPx { get; set; } = 2f;
    public int CircleSegments { get; set; } = 16;
    public FloatColor4 ScanPointColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#FF3333", nameof(ScanPointColor));
    public FloatColor4 OriginMarkerColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#FFFFFF", nameof(OriginMarkerColor));
    public FloatColor4 OffScaleIndicatorColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#F2261F", nameof(OffScaleIndicatorColor));
    public OffScaleIndicatorSettings OffScaleIndicator { get; set; } = new();
}
