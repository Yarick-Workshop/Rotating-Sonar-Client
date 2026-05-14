namespace Rotating.Sonar.Client.Tests.Settings.Fixtures;

public sealed class CustomSettingsWithEnumerableProperty
{
    public List<int> Numbers { get; set; } = new() { 1 };
}
