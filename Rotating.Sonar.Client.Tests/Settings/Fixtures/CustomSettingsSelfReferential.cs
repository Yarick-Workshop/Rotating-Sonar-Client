namespace Rotating.Sonar.Client.Tests.Settings.Fixtures;

public sealed class CustomSettingsSelfReferential
{
    public string Label { get; set; } = "root";

    public CustomSettingsSelfReferential? Owner { get; set; }
}
