namespace Rotating.Sonar.Client.Common.Settings;

using System.ComponentModel.DataAnnotations;
using System.Globalization;

public sealed class FakeSerialLineFormatter
{
    private readonly string _compositeFormat;

    public FakeSerialLineFormatter(string namedPlaceholderTemplate)
    {
        ArgumentNullException.ThrowIfNull(namedPlaceholderTemplate);
        this._compositeFormat = BuildCompositeFormat(namedPlaceholderTemplate);
    }

    public string CreateLine(int angleDeg, int distanceCm) =>
        string.Format(CultureInfo.InvariantCulture, this._compositeFormat, angleDeg, distanceCm);

    public static IEnumerable<ValidationResult> ValidateNamedPlaceholderTemplate(
        string namedPlaceholderTemplate,
        params string[] memberNames)
    {
        if (string.IsNullOrWhiteSpace(namedPlaceholderTemplate))
        {
            yield return new ValidationResult(
                "Fake serial line format is required.",
                memberNames);
            yield break;
        }

        string anglePh = $"{{{SerialLineParseConstants.AngleGroupName}}}";
        string distancePh = $"{{{SerialLineParseConstants.DistanceGroupName}}}";
        bool missingAngle = !namedPlaceholderTemplate.Contains(anglePh, StringComparison.Ordinal);
        bool missingDistance = !namedPlaceholderTemplate.Contains(distancePh, StringComparison.Ordinal);
        if (missingAngle)
        {
            yield return new ValidationResult(
                $"The format must include '{anglePh}' matching the line pattern capture name.",
                memberNames);
        }

        if (missingDistance)
        {
            yield return new ValidationResult(
                $"The format must include '{distancePh}' matching the line pattern capture name.",
                memberNames);
        }

        if (missingAngle || missingDistance)
        {
            yield break;
        }

        FormatException? formatException = null;
        try
        {
            _ = new FakeSerialLineFormatter(namedPlaceholderTemplate).CreateLine(0, 0);
        }
        catch (FormatException ex)
        {
            formatException = ex;
        }

        if (formatException is not null)
        {
            yield return new ValidationResult(
                FormatCompositeValidationMessage(formatException),
                memberNames);
        }
    }

    public static string FormatCompositeValidationMessage(FormatException formatException)
    {
        ArgumentNullException.ThrowIfNull(formatException);
        return $"Fake serial line format is not a valid composite format string after named placeholders were replaced with indices: {formatException.Message}";
    }

    private static string BuildCompositeFormat(string namedPlaceholderTemplate)
    {
        (string Placeholder, string IndexToken)[] map =
        [
            ($"{{{SerialLineParseConstants.AngleGroupName}}}", "{0}"),
            ($"{{{SerialLineParseConstants.DistanceGroupName}}}", "{1}"),
        ];

        string result = namedPlaceholderTemplate;
        foreach ((string placeholder, string indexToken) in map)
        {
            result = result.Replace(placeholder, indexToken, StringComparison.Ordinal);
        }

        return result;
    }
}
