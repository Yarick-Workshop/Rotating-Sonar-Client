namespace Rotating.Sonar.Client.Common.Settings;

using System.ComponentModel.DataAnnotations;

public sealed class FakeSerialDataSettings : IValidatableObject
{
    public int MinAngleDeg { get; set; } = -90;

    public int MaxAngleDeg { get; set; } = 90;

    [Range(1, int.MaxValue, ErrorMessage = "AngleStepDeg must be at least 1 to avoid infinite loops.")]
    public int AngleStepDeg { get; set; } = 15;

    [Range(0, int.MaxValue, ErrorMessage = "BaseDistanceCm must be non-negative.")]
    public int BaseDistanceCm { get; set; } = 120;

    [Range(0, int.MaxValue, ErrorMessage = "DistanceJitterCm must be non-negative.")]
    public int DistanceJitterCm { get; set; } = 10;

    [Range(0, int.MaxValue, ErrorMessage = "IntervalMilliseconds must be non-negative.")]
    public int IntervalMilliseconds { get; set; } = 100;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Fake serial line format is required.")]
    public string FakeSerialLineFormat { get; set; } =
        $"{{{SerialLineParseConstants.AngleGroupName}}}: {{{SerialLineParseConstants.DistanceGroupName}}}cm";

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (this.MinAngleDeg > this.MaxAngleDeg)
        {
            yield return new ValidationResult(
                "MinAngleDeg must be less than or equal to MaxAngleDeg.",
                [nameof(this.MinAngleDeg), nameof(this.MaxAngleDeg)]);
        }

        foreach (ValidationResult result in FakeSerialLineFormatter.ValidateNamedPlaceholderTemplate(
            this.FakeSerialLineFormat,
            nameof(this.FakeSerialLineFormat)))
        {
            yield return result;
        }
    }
}
