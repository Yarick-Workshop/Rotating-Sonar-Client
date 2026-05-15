namespace Rotating.Sonar.Client.Common.Settings;

using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

public sealed class SerialSettings : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "DefaultBaudRate must be at least 1.")]
    public int DefaultBaudRate { get; set; } = 9600;

    [Range(0, int.MaxValue, ErrorMessage = "ReadTimeoutMilliseconds must be non-negative.")]
    public int ReadTimeoutMilliseconds { get; set; } = 250;

    [Range(1, SerialLineParseConstants.LinePatternMatchTimeoutMillisecondsMax, ErrorMessage = "LinePatternMatchTimeoutMilliseconds must be between 1 and 1000 milliseconds (1 second maximum).")]
    public int LinePatternMatchTimeoutMilliseconds { get; set; } = 100;

    [Required(AllowEmptyStrings = false, ErrorMessage = "Serial line pattern is required.")]
    public string LinePattern { get; set; } =
        $"(?<{SerialLineParseConstants.AngleGroupName}>[+-]?\\d+):\\s*(?<{SerialLineParseConstants.DistanceGroupName}>\\d+)cm";

    [Required(ErrorMessage = "Serial fake data settings are required.")]
    public FakeSerialDataSettings FakeData { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        Regex regex;
        try
        {
            regex = new Regex(
                this.LinePattern,
                RegexOptions.None,
                TimeSpan.FromMilliseconds(this.LinePatternMatchTimeoutMilliseconds));
        }
        catch (ArgumentException ex)
        {
            return new[]
            {
                new ValidationResult(
                    $"Serial line pattern is not a valid regular expression: {ex.Message}",
                    new[] { nameof(this.LinePattern) }),
            };
        }

        string[] definedNames = regex.GetGroupNames();
        foreach (string requiredName in new[] { SerialLineParseConstants.AngleGroupName, SerialLineParseConstants.DistanceGroupName })
        {
            bool found = definedNames.Any(n => string.Equals(n, requiredName, StringComparison.Ordinal));
            if (!found)
            {
                return new[]
                {
                    new ValidationResult(
                        $"Named capture group '{requiredName}' is not defined by the pattern (pattern defines groups: {string.Join(", ", definedNames)}).",
                        new[] { nameof(this.LinePattern) }),
                };
            }
        }

        var fakeLineFormatter = new FakeSerialLineFormatter(this.FakeData.FakeSerialLineFormat);

        foreach ((int angleDeg, int distanceCm) in new[] { (-12, 99), (0, 0), (45, 120), (-90, 1) })
        {
            string line;
            try
            {
                line = fakeLineFormatter.CreateLine(angleDeg, distanceCm);
            }
            catch (FormatException ex)
            {
                return new[]
                {
                    new ValidationResult(
                        FakeSerialLineFormatter.FormatCompositeValidationMessage(ex),
                        new[] { $"{nameof(this.FakeData)}.{nameof(FakeSerialDataSettings.FakeSerialLineFormat)}" }),
                };
            }

            Match match = regex.Match(line);
            if (!match.Success)
            {
                return new[]
                {
                    new ValidationResult(
                        $"Fake serial line format and line pattern do not agree: formatted sample \"{line}\" does not match the regular expression.",
                        new[] { nameof(this.LinePattern), $"{nameof(this.FakeData)}.{nameof(FakeSerialDataSettings.FakeSerialLineFormat)}" }),
                };
            }

            if (!int.TryParse(match.Groups[SerialLineParseConstants.AngleGroupName].Value, out int parsedAngle)
                || !int.TryParse(match.Groups[SerialLineParseConstants.DistanceGroupName].Value, out int parsedDistance)
                || parsedAngle != angleDeg
                || parsedDistance != distanceCm)
            {
                return new[]
                {
                    new ValidationResult(
                        $"Fake serial line format and line pattern do not agree: for sample ({angleDeg}, {distanceCm}) the line \"{line}\" matched but capture groups did not round-trip the values.",
                        new[] { nameof(this.LinePattern), $"{nameof(this.FakeData)}.{nameof(FakeSerialDataSettings.FakeSerialLineFormat)}" }),
                };
            }
        }

        return Enumerable.Empty<ValidationResult>();
    }
}
