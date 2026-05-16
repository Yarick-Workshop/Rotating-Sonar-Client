namespace Rotating.Sonar.Client.Common.Settings;

using System.ComponentModel.DataAnnotations;
using Serilog.Events;

public sealed class LoggingSettings : IValidatableObject
{
    public LogEventLevel Visualization { get; set; } = LogEventLevel.Information;

    public LogEventLevel ConsoleOnly { get; set; } = LogEventLevel.Debug;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (ValidationResult result in ValidateLogEventLevel(nameof(this.Visualization), this.Visualization))
        {
            yield return result;
        }

        foreach (ValidationResult result in ValidateLogEventLevel(nameof(this.ConsoleOnly), this.ConsoleOnly))
        {
            yield return result;
        }
    }

    private static IEnumerable<ValidationResult> ValidateLogEventLevel(string propertyName, LogEventLevel level)
    {
        if (Enum.IsDefined(level))
        {
            yield break;
        }

        yield return new ValidationResult(
            $"{propertyName} must be a Serilog log level ({string.Join(", ", Enum.GetNames<LogEventLevel>())}).",
            [propertyName]);
    }
}
