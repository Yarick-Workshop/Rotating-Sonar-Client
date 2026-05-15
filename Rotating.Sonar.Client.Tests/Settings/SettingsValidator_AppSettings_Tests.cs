namespace Rotating.Sonar.Client.Tests.Settings;

using Rotating.Sonar.Client.Common.Settings;

[TestFixture]
public sealed class SettingsValidator_AppSettings_Tests
{
    [Test]
    public void ValidateRecursively_AppSettingsValidDefaults_DoesNotThrow()
    {
        var settings = new AppSettings();

        Assert.DoesNotThrow(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(AppSettings)));
    }

    [Test]
    public void ValidateRecursively_AppSettingsInvalidSerialBaudRate_ThrowsWithSerialPath()
    {
        var settings = new AppSettings();
        settings.Serial.DefaultBaudRate = 0;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(AppSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'AppSettings.Serial.DefaultBaudRate':"));
    }

    [Test]
    public void ValidateRecursively_AppSettingsSerialFakeDataNull_ThrowsWithRequiredMessage()
    {
        var settings = new AppSettings();
        settings.Serial.FakeData = null!;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(AppSettings)));

        Assert.That(ex!.Message, Does.Contain(
            "at 'AppSettings.Serial.FakeData': Serial fake data settings are required."));
    }

    [Test]
    public void ValidateRecursively_AppSettingsFakeDataMinAngleGreaterThanMax_ThrowsWithFakeDataPath()
    {
        var settings = new AppSettings();
        settings.Serial.FakeData.MinAngleDeg = 100;
        settings.Serial.FakeData.MaxAngleDeg = 50;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(AppSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'AppSettings.Serial.FakeData.MinAngleDeg':"));
    }

    [Test]
    public void ValidateRecursively_AppSettingsFakeDataZeroAngleStep_ThrowsWithRangeMessage()
    {
        var settings = new AppSettings();
        settings.Serial.FakeData.AngleStepDeg = 0;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(AppSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'AppSettings.Serial.FakeData.AngleStepDeg':"));
    }

    [Test]
    public void ValidateRecursively_AppSettingsSerialLinePatternMatchTimeoutOutOfRange_ThrowsWithSerialPath()
    {
        var settings = new AppSettings();
        settings.Serial.LinePatternMatchTimeoutMilliseconds = 0;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(AppSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'AppSettings.Serial.LinePatternMatchTimeoutMilliseconds':"));
    }

    [Test]
    public void ValidateRecursively_AppSettingsSerialLinePatternEmpty_ThrowsWithSerialPath()
    {
        var settings = new AppSettings();
        settings.Serial.LinePattern = string.Empty;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(AppSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'AppSettings.Serial.LinePattern':"));
    }

    [Test]
    public void ValidateRecursively_AppSettingsSerialLinePatternInvalidRegex_ThrowsWithSerialPath()
    {
        var settings = new AppSettings();
        settings.Serial.LinePattern = "(";

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(AppSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'AppSettings.Serial.LinePattern':"));
    }

    [Test]
    public void ValidateRecursively_AppSettingsSerialFakeFormatAndLinePatternMismatch_ThrowsWithSerialPaths()
    {
        var settings = new AppSettings();
        settings.Serial.FakeData.FakeSerialLineFormat = "{angle};{distance}";

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(AppSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'AppSettings.Serial.LinePattern':"));
        Assert.That(ex.Message, Does.Contain("at 'AppSettings.Serial.FakeData.FakeSerialLineFormat':"));
    }

    [Test]
    public void ValidateRecursively_AppSettingsWindowInvalidTitle_ThrowsWithRequiredMessage()
    {
        var settings = new AppSettings();
        settings.Visualizer.Window.Title = null!;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(AppSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'AppSettings.Visualizer.Window.Title':"));
    }

    [Test]
    public void ValidateRecursively_AppSettingsZoomDefaultScaleOutOfRange_ThrowsWithZoomPath()
    {
        var settings = new AppSettings();
        settings.Visualizer.Zoom.MinScale = 1f;
        settings.Visualizer.Zoom.MaxScale = 2f;
        settings.Visualizer.Zoom.DefaultScale = 3f;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(AppSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'AppSettings.Visualizer.Zoom.DefaultScale':"));
    }

    [Test]
    public void ValidateRecursively_AppSettingsSweepConeAlphaOutOfRange_ThrowsWithSweepPath()
    {
        var settings = new AppSettings();
        settings.Visualizer.Sweep.ConeCenterAlpha = 1.5f;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(AppSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'AppSettings.Visualizer.Sweep.ConeCenterAlpha':"));
    }

    [Test]
    public void ValidateRecursively_AppSettingsRangeGridTickStepOutOfRange_ThrowsWithRangeGridPath()
    {
        var settings = new AppSettings();
        settings.Visualizer.RangeGrid.TickStepDeg = 0;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(AppSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'AppSettings.Visualizer.RangeGrid.TickStepDeg':"));
    }

    [Test]
    public void ValidateRecursively_AppSettingsScanPointsNegativePointSize_ThrowsWithScanPointsPath()
    {
        var settings = new AppSettings();
        settings.Visualizer.ScanPoints.PointSizePx = 0f;

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(AppSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'AppSettings.Visualizer.ScanPoints.PointSizePx':"));
    }

    [Test]
    public void Load_InvalidSettingsPath_ReturnsDefaultsPassingRecursiveValidation()
    {
        AppSettings settings = AppSettings.Load(" ");

        Assert.DoesNotThrow(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(AppSettings)));
    }
}
