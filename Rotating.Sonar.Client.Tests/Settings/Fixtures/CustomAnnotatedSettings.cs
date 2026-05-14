namespace Rotating.Sonar.Client.Tests.Settings.Fixtures;

using System.ComponentModel.DataAnnotations;

public sealed class CustomAnnotatedSettings
{
    [Required(ErrorMessage = "Label is required.")]
    public string Label { get; set; } = "default";

    [Range(1, 100, ErrorMessage = "Port must be between 1 and 100.")]
    public int Port { get; set; } = 80;

    public CustomAnnotatedNestedSection Section { get; set; } = new();

    public CustomCrossValidatedSettings Bounds { get; set; } = new();
}
