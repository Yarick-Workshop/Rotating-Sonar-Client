namespace Rotating.Sonar.Client.Common.Settings;

public sealed class ScanAreaSettings
{
    public float MaxDistanceCm { get; set; } = 200f;

    public float OuterMarginPx { get; set; } = 40f;

    public float OverlayMarginPx { get; set; } = 10f;
}
