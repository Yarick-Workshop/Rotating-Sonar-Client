namespace Rotating.Sonar.Client.Tests.Settings.Fixtures;

public sealed class CustomSettingsWithArrayMember
{
    public CustomAnnotatedNestedSection[] Blocks { get; set; } = { new() };
}
