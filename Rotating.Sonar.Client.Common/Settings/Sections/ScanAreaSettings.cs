namespace Rotating.Sonar.Client.Common.Settings.Sections;

using System.ComponentModel.DataAnnotations;

public sealed class ScanAreaSettings
{
    [Range(0.01, float.MaxValue, ErrorMessage = "MaxDistanceCm must be positive.")]
    public float MaxDistanceCm { get; set; } = 200f;

    [Range(0, float.MaxValue, ErrorMessage = "OuterMarginPx must be non-negative.")]
    public float OuterMarginPx { get; set; } = 40f;

    [Range(0, float.MaxValue, ErrorMessage = "OverlayMarginPx must be non-negative.")]
    public float OverlayMarginPx { get; set; } = 10f;
}
