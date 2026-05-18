namespace Rotating.Sonar.Client.Tests.Settings;

using System.ComponentModel.DataAnnotations;
using Rotating.Sonar.Client.Common.Settings;

[TestFixture]
public sealed class FakeSerialLineFormatter_Tests
{
    [Test]
    public void Constructor_NullTemplate_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new FakeSerialLineFormatter(null!));
    }

    [Test]
    public void CreateLine_DefaultTemplate_FormatsAngleAndDistance()
    {
        var formatter = new FakeSerialLineFormatter(
            $"{{{SerialLineParseConstants.AngleGroupName}}}: {{{SerialLineParseConstants.DistanceGroupName}}}cm");

        string line = formatter.CreateLine(-12, 99);

        Assert.That(line, Is.EqualTo("-12: 99cm"));
    }

    [Test]
    public void CreateLine_UsesInvariantCulture_RegardlessOfThreadCulture()
    {
        var formatter = new FakeSerialLineFormatter(
            $"{{{SerialLineParseConstants.AngleGroupName}}}|{{{SerialLineParseConstants.DistanceGroupName}}}");

        string line = formatter.CreateLine(0, 0);

        Assert.That(line, Is.EqualTo("0|0"));
    }

    [Test]
    public void ValidateNamedPlaceholderTemplate_Empty_ReturnsRequiredMessage()
    {
        var results = FakeSerialLineFormatter
            .ValidateNamedPlaceholderTemplate(string.Empty, "fmt")
            .ToList();

        Assert.That(results, Has.Count.EqualTo(1));
        Assert.That(results[0].ErrorMessage, Does.Contain("required"));
    }

    [Test]
    public void ValidateNamedPlaceholderTemplate_MissingAngle_ReturnsAngleError()
    {
        var results = FakeSerialLineFormatter
            .ValidateNamedPlaceholderTemplate(
                $"{{{SerialLineParseConstants.DistanceGroupName}}}cm",
                "fmt")
            .ToList();

        Assert.That(results, Has.Some.Matches<ValidationResult>(r =>
            r.ErrorMessage!.Contains(SerialLineParseConstants.AngleGroupName)));
    }

    [Test]
    public void ValidateNamedPlaceholderTemplate_MissingDistance_ReturnsDistanceError()
    {
        var results = FakeSerialLineFormatter
            .ValidateNamedPlaceholderTemplate(
                $"{{{SerialLineParseConstants.AngleGroupName}}}",
                "fmt")
            .ToList();

        Assert.That(results, Has.Some.Matches<ValidationResult>(r =>
            r.ErrorMessage!.Contains(SerialLineParseConstants.DistanceGroupName)));
    }

    [Test]
    public void ValidateNamedPlaceholderTemplate_UnknownNumericPlaceholder_ReturnsCompositeFormatError()
    {
        // Replacing the named placeholders leaves a stray {7} which makes the composite format
        // throw FormatException — exercises the inlined composite-format error message.
        string template = $"{{{SerialLineParseConstants.AngleGroupName}}}: {{{SerialLineParseConstants.DistanceGroupName}}} {{7}}";

        var results = FakeSerialLineFormatter
            .ValidateNamedPlaceholderTemplate(template, "fmt")
            .ToList();

        Assert.That(results, Has.Some.Matches<ValidationResult>(r =>
            r.ErrorMessage!.Contains("after named placeholders were replaced with indices")));
    }

    [Test]
    public void ValidateNamedPlaceholderTemplate_ValidTemplate_ReturnsNoResults()
    {
        string template = $"{{{SerialLineParseConstants.AngleGroupName}}}: {{{SerialLineParseConstants.DistanceGroupName}}}cm";

        var results = FakeSerialLineFormatter
            .ValidateNamedPlaceholderTemplate(template, "fmt")
            .ToList();

        Assert.That(results, Is.Empty);
    }

    [Test]
    public void FormatCompositeValidationMessage_NullException_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            FakeSerialLineFormatter.FormatCompositeValidationMessage(null!));
    }

    [Test]
    public void FormatCompositeValidationMessage_FromFormatException_IncludesInnerMessage()
    {
        var ex = new FormatException("inner detail");

        string message = FakeSerialLineFormatter.FormatCompositeValidationMessage(ex);

        Assert.That(message, Does.Contain("inner detail"));
    }
}
