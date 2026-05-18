namespace Rotating.Sonar.Client.Tests.Settings.Fixtures;

using Rotating.Sonar.Client.Common.Settings;

public sealed class CustomSettingsWithFloatColor4
{
    public FloatColor4 Color { get; set; } = new(0.5f, 0.5f, 0.5f, 1f);
}
