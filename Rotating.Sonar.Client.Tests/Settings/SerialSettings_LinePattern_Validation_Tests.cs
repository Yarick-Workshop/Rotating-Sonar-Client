namespace Rotating.Sonar.Client.Tests.Settings;

using System.Globalization;
using Rotating.Sonar.Client.Common.Settings;

[TestFixture]
public sealed class SerialSettings_LinePattern_Validation_Tests
{
    private static SerialSettings CreateValidSerial() => new();

    [Test]
    public void ValidateRecursively_DefaultSerialSettings_Passes_AndFakeLineMatchesRegex()
    {
        var serial = CreateValidSerial();

        Assert.DoesNotThrow(() =>
            SettingsValidator.ValidateRecursively(serial, nameof(SerialSettings)));

        string composite = serial.FakeData.FakeSerialLineFormat
            .Replace($"{{{SerialLineParseConstants.DistanceGroupName}}}", "{1}", StringComparison.Ordinal)
            .Replace($"{{{SerialLineParseConstants.AngleGroupName}}}", "{0}", StringComparison.Ordinal);
        string line = string.Format(CultureInfo.InvariantCulture, composite, -12, 99);
        var match = serial.CreateLineRegex().Match(line);
        Assert.That(match.Success, Is.True);
        Assert.That(int.Parse(match.Groups[SerialLineParseConstants.AngleGroupName].Value), Is.EqualTo(-12));
        Assert.That(int.Parse(match.Groups[SerialLineParseConstants.DistanceGroupName].Value), Is.EqualTo(99));
    }

    [Test]
    public void ValidateRecursively_LinePatternNull_FailsRequiredValidation()
    {
        var serial = CreateValidSerial();
        serial.LinePattern = null!;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(serial, nameof(SerialSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'SerialSettings.LinePattern':"));
    }

    [Test]
    public void ValidateRecursively_LinePatternEmpty_FailsRequiredValidation()
    {
        var serial = CreateValidSerial();
        serial.LinePattern = string.Empty;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(serial, nameof(SerialSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'SerialSettings.LinePattern':"));
    }

    [Test]
    public void ValidateRecursively_LinePatternInvalidSyntax_FailsCompileValidation()
    {
        var serial = CreateValidSerial();
        serial.LinePattern = "(";

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(serial, nameof(SerialSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'SerialSettings.LinePattern':"));
        Assert.That(ex.Message, Does.Contain("not a valid regular expression"));
    }

    [Test]
    public void ValidateRecursively_LinePatternMissingAngleNamedGroup_FailsNamedGroupValidation()
    {
        var serial = CreateValidSerial();
        serial.LinePattern = $@"(?<{SerialLineParseConstants.DistanceGroupName}>\d+)cm";

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(serial, nameof(SerialSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'SerialSettings.LinePattern':"));
        Assert.That(ex.Message, Does.Contain(SerialLineParseConstants.AngleGroupName));
    }

    [Test]
    public void ValidateRecursively_LinePatternMissingDistanceNamedGroup_FailsNamedGroupValidation()
    {
        var serial = CreateValidSerial();
        serial.LinePattern = $@"(?<{SerialLineParseConstants.AngleGroupName}>[+-]?\d+):\s*\d+cm";

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(serial, nameof(SerialSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'SerialSettings.LinePattern':"));
        Assert.That(ex.Message, Does.Contain(SerialLineParseConstants.DistanceGroupName));
    }

    [Test]
    public void ValidateRecursively_FakeDataFakeSerialLineFormatNull_FailsRequiredValidation()
    {
        var serial = CreateValidSerial();
        serial.FakeData.FakeSerialLineFormat = null!;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(serial, nameof(SerialSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'SerialSettings.FakeData.FakeSerialLineFormat':"));
    }

    [Test]
    public void ValidateRecursively_FakeDataFakeSerialLineFormatEmpty_FailsRequiredValidation()
    {
        var serial = CreateValidSerial();
        serial.FakeData.FakeSerialLineFormat = string.Empty;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(serial, nameof(SerialSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'SerialSettings.FakeData.FakeSerialLineFormat':"));
    }

    [Test]
    public void ValidateRecursively_FakeDataFakeSerialLineFormatUnclosedBrace_FailsValidation()
    {
        var serial = CreateValidSerial();
        serial.FakeData.FakeSerialLineFormat = "{angle";

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(serial, nameof(SerialSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'SerialSettings.FakeData.FakeSerialLineFormat':"));
        Assert.That(ex.Message, Does.Contain("must include"));
        Assert.That(ex.Message, Does.Contain(SerialLineParseConstants.AngleGroupName));
    }

    [Test]
    public void ValidateRecursively_FakeDataFakeSerialLineFormatNumericPlaceholders_FailsValidation()
    {
        var serial = CreateValidSerial();
        serial.FakeData.FakeSerialLineFormat = "{0}: {1}cm";

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(serial, nameof(SerialSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'SerialSettings.FakeData.FakeSerialLineFormat':"));
        Assert.That(ex.Message, Does.Contain("must include"));
        Assert.That(ex.Message, Does.Contain(SerialLineParseConstants.AngleGroupName));
    }

    [Test]
    public void ValidateRecursively_FakeDataFakeSerialLineFormatUnknownPlaceholder_FailsValidation()
    {
        var serial = CreateValidSerial();
        serial.FakeData.FakeSerialLineFormat =
            $"{{{SerialLineParseConstants.AngleGroupName}}}: {{{SerialLineParseConstants.DistanceGroupName}}} {{foo}}";

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(serial, nameof(SerialSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'SerialSettings.FakeData.FakeSerialLineFormat':"));
        Assert.That(ex.Message, Does.Contain("after named placeholders were replaced with indices"));
    }

    [Test]
    public void ValidateRecursively_LinePatternAndFakeFormatMismatch_FailsCrossValidation()
    {
        var serial = CreateValidSerial();
        serial.FakeData.FakeSerialLineFormat =
            $"{{{SerialLineParseConstants.AngleGroupName}}};{{{SerialLineParseConstants.DistanceGroupName}}}";

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(serial, nameof(SerialSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'SerialSettings.LinePattern':"));
        Assert.That(ex.Message, Does.Contain("at 'SerialSettings.FakeData.FakeSerialLineFormat':"));
        Assert.That(ex.Message, Does.Contain("do not agree"));
    }

    [Test]
    public void ValidateRecursively_LinePatternMatchTimeoutZero_FailsRangeValidation()
    {
        var serial = CreateValidSerial();
        serial.LinePatternMatchTimeoutMilliseconds = 0;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(serial, nameof(SerialSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'SerialSettings.LinePatternMatchTimeoutMilliseconds':"));
    }

    [Test]
    public void ValidateRecursively_LinePatternMatchTimeoutAboveMax_FailsRangeValidation()
    {
        var serial = CreateValidSerial();
        serial.LinePatternMatchTimeoutMilliseconds = SerialLineParseConstants.LinePatternMatchTimeoutMillisecondsMax + 1;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(serial, nameof(SerialSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'SerialSettings.LinePatternMatchTimeoutMilliseconds':"));
    }
}
