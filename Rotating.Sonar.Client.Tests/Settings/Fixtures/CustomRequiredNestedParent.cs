namespace Rotating.Sonar.Client.Tests.Settings.Fixtures;

using System.ComponentModel.DataAnnotations;

public sealed class CustomRequiredNestedParent
{
    [Required(ErrorMessage = "Detail section is required.")]
    public CustomAnnotatedNestedSection? Detail { get; set; }
}
