namespace Rotating.Sonar.Client.Tests.Settings.Fixtures;

using System.ComponentModel.DataAnnotations;

public sealed class CustomSettingsWithWhitespaceValidationMessage : IValidatableObject
{
    public string Label { get; set; } = "ok";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        yield return new ValidationResult("   ", [nameof(this.Label)]);
    }
}
