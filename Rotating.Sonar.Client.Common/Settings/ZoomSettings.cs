namespace Rotating.Sonar.Client.Common.Settings;

public sealed class ZoomSettings
{
    public bool ShowByDefault { get; set; } = true;

    public float MinScale { get; set; } = 0.25f;

    public float MaxScale { get; set; } = 4f;

    public float DefaultScale { get; set; } = 1f;

    public float FactorPerStep { get; set; } = 1.12f;

    public float MaxWheelExponent { get; set; } = 5f;
}
