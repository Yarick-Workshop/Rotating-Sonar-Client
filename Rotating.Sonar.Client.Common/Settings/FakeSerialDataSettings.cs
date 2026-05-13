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

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (this.MinAngleDeg > this.MaxAngleDeg)
        {
            yield return new ValidationResult(
                "MinAngleDeg must be less than or equal to MaxAngleDeg.",
                [nameof(this.MinAngleDeg), nameof(this.MaxAngleDeg)]);
        }
    }
}
