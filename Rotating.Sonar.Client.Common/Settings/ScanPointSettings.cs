namespace Rotating.Sonar.Client.Common.Settings;

using System.ComponentModel.DataAnnotations;

public sealed class ScanPointSettings
{
    [Range(0.01, float.MaxValue, ErrorMessage = "PointSizePx must be positive.")]
    public float PointSizePx { get; set; } = 12f;

    [Range(0.01, float.MaxValue, ErrorMessage = "LineWidthPx must be positive.")]
    public float LineWidthPx { get; set; } = 2f;

    [Range(3, int.MaxValue, ErrorMessage = "CircleSegments must be at least 3.")]
    public int CircleSegments { get; set; } = 16;

    public FloatColor4 ScanPointColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#FF3333", nameof(ScanPointColor));
    public FloatColor4 OriginMarkerColor { get; set; } = ChainColorParser.Instance.ParseOrThrow("#FFFFFF", nameof(OriginMarkerColor));
    public OutOfRangeArrowSettings OutOfRangeArrow { get; set; } = new();
}
