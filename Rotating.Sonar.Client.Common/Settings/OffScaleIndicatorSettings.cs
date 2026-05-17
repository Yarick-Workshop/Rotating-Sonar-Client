namespace Rotating.Sonar.Client.Common.Settings;

using System.ComponentModel.DataAnnotations;

public sealed class OffScaleIndicatorSettings
{
    public FloatColor4 Color { get; set; } = ChainColorParser.Instance.ParseOrThrow("#F2261F", nameof(Color));

    [Range(0, float.MaxValue, ErrorMessage = "ArrowHeadLengthPx must be non-negative.")]
    public float ArrowHeadLengthPx { get; set; } = 8f;

    [Range(0, float.MaxValue, ErrorMessage = "ArrowHalfWidthPx must be non-negative.")]
    public float ArrowHalfWidthPx { get; set; } = 5f;

    [Range(0, float.MaxValue, ErrorMessage = "RimInsetPx must be non-negative.")]
    public float RimInsetPx { get; set; } = 8f;

    [Range(0, float.MaxValue, ErrorMessage = "MinTailRadiusPx must be non-negative.")]
    public float MinTailRadiusPx { get; set; } = 26f;

    [Range(0, float.MaxValue, ErrorMessage = "MinArrowHeadBackPx must be non-negative.")]
    public float MinArrowHeadBackPx { get; set; } = 2.5f;
}
