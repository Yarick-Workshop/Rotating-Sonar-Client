namespace Rotating.Sonar.Client.Common.Settings;

public sealed class ScanPointSettings
{
    public float PointSizePx { get; set; } = 12f;
    public FloatColor4 ScanPointColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#FF3333", nameof(ScanPointColor));
    public FloatColor4 OriginMarkerColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#FFFFFF", nameof(OriginMarkerColor));
    public FloatColor4 OffScaleIndicatorColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#F2261F", nameof(OffScaleIndicatorColor));
}
