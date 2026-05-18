namespace Rotating.Sonar.Client.Tests.Settings.Fixtures;

public sealed class CustomSettingsSelfReferentialParent
{
    public string Label { get; set; } = "root";

    public CustomSettingsSelfReferentialParent? Parent { get; set; }
}
