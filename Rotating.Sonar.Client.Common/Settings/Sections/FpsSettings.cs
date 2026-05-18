namespace Rotating.Sonar.Client.Common.Settings.Sections;

using System.ComponentModel.DataAnnotations;

public sealed class FpsSettings
{
    public bool ShowByDefault { get; set; } = true;

    [Range(1, int.MaxValue, ErrorMessage = "HistorySize must be at least 1.")]
    public int HistorySize { get; set; } = 60;

    [Range(0, float.MaxValue, ErrorMessage = "OffsetXPx must be non-negative.")]
    public float OffsetXPx { get; set; } = 10f;

    [Range(0, float.MaxValue, ErrorMessage = "OffsetFromTopPx must be non-negative.")]
    public float OffsetFromTopPx { get; set; } = 40f;
}
