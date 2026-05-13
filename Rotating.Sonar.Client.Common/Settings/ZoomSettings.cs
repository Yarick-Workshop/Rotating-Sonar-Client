namespace Rotating.Sonar.Client.Common.Settings;

using System.ComponentModel.DataAnnotations;

public sealed class ZoomSettings : IValidatableObject
{
    public bool ShowByDefault { get; set; } = true;

    [Range(0.01, float.MaxValue, ErrorMessage = "MinScale must be positive.")]
    public float MinScale { get; set; } = 0.25f;

    [Range(0.01, float.MaxValue, ErrorMessage = "MaxScale must be positive.")]
    public float MaxScale { get; set; } = 4f;

    [Range(0.01, float.MaxValue, ErrorMessage = "DefaultScale must be positive.")]
    public float DefaultScale { get; set; } = 1f;

    [Range(1.001, float.MaxValue, ErrorMessage = "FactorPerStep must be greater than 1.")]
    public float FactorPerStep { get; set; } = 1.12f;

    [Range(0.01, float.MaxValue, ErrorMessage = "MaxWheelExponent must be positive.")]
    public float MaxWheelExponent { get; set; } = 5f;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (this.MinScale > this.MaxScale)
        {
            yield return new ValidationResult(
                "MinScale must be less than or equal to MaxScale.",
                [nameof(this.MinScale), nameof(this.MaxScale)]);
        }

        if (this.DefaultScale < this.MinScale || this.DefaultScale > this.MaxScale)
        {
            yield return new ValidationResult(
                "DefaultScale must be within [MinScale..MaxScale].",
                [nameof(this.DefaultScale)]);
        }
    }
}
