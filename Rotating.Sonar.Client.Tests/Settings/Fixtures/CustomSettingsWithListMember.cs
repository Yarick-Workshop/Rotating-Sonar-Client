namespace Rotating.Sonar.Client.Tests.Settings.Fixtures;

public sealed class CustomSettingsWithListMember
{
    public List<CustomAnnotatedNestedSection> Sections { get; set; } = new() { new() };
}
