namespace Rotating.Sonar.Client.Common.Settings;

public sealed class FpsSettings
{
    public bool ShowByDefault { get; set; } = true;

    public int HistorySize { get; set; } = 60;

    public float OffsetXPx { get; set; } = 10f;

    public float OffsetFromTopPx { get; set; } = 40f;
}
