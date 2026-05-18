namespace Rotating.Sonar.Client.Tests.Settings.Fixtures;

using System.ComponentModel.DataAnnotations;

public sealed class CustomCrossValidatedSettings : IValidatableObject
{
    [Range(0, 100, ErrorMessage = "Low must be between 0 and 100.")]
    public int Low { get; set; } = 0;

    [Range(0, 100, ErrorMessage = "High must be between 0 and 100.")]
    public int High { get; set; } = 10;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (this.Low > this.High)
        {
            yield return new ValidationResult(
                "Low must be less than or equal to High.",
                [nameof(this.Low), nameof(this.High)]);
        }
    }
}
