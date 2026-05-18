namespace Rotating.Sonar.Client.Tests.Settings.Fixtures;

using System.ComponentModel.DataAnnotations;

public sealed class CustomAnnotatedNestedSection
{
    [Range(0, 10, ErrorMessage = "Level must be between 0 and 10.")]
    public int Level { get; set; } = 5;

    [StringLength(4, MinimumLength = 2, ErrorMessage = "Code must be between 2 and 4 characters.")]
    public string Code { get; set; } = "ab";
}
