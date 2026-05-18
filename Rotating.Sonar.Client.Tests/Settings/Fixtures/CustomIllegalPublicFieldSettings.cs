namespace Rotating.Sonar.Client.Tests.Settings.Fixtures;

/// <summary>Used to test rejection of public instance fields on settings types.</summary>
public sealed class CustomIllegalPublicFieldSettings
{
    public int Okay { get; set; }

    public int DisallowedPublicField;
}
