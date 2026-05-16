namespace Rotating.Sonar.Client.Tests.Settings;

using Microsoft.Extensions.Configuration;
using Rotating.Sonar.Client.Common.Settings;
using Serilog.Events;

[TestFixture]
public sealed class LoggingSettings_Tests
{
    [Test]
    public void Defaults_AreInformationForVisualizationAndDebugForConsoleOnly()
    {
        var settings = new LoggingSettings();

        Assert.That(settings.Visualization, Is.EqualTo(LogEventLevel.Information));
        Assert.That(settings.ConsoleOnly, Is.EqualTo(LogEventLevel.Debug));
    }

    [Test]
    public void ValidateRecursively_UndefinedVisualizationLevel_ThrowsWithPropertyPath()
    {
        var settings = new LoggingSettings
        {
            Visualization = (LogEventLevel)999,
        };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(LoggingSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'LoggingSettings.Visualization':"));
        Assert.That(ex.Message, Does.Contain("Serilog log level"));
    }

    [Test]
    public void ValidateRecursively_UndefinedConsoleOnlyLevel_ThrowsWithPropertyPath()
    {
        var settings = new LoggingSettings
        {
            ConsoleOnly = (LogEventLevel)999,
        };

        var ex = Assert.Throws<InvalidOperationException>(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(LoggingSettings)));

        Assert.That(ex!.Message, Does.Contain("at 'LoggingSettings.ConsoleOnly':"));
        Assert.That(ex.Message, Does.Contain("Serilog log level"));
    }

    [Test]
    public void ValidateRecursively_ValidLevels_DoesNotThrow()
    {
        var settings = new LoggingSettings
        {
            Visualization = LogEventLevel.Warning,
            ConsoleOnly = LogEventLevel.Error,
        };

        Assert.DoesNotThrow(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(LoggingSettings)));
    }

    [Test]
    public void BindFromConfiguration_ParsesSerilogLevelNames()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{nameof(AppSettings.Logging)}:Visualization"] = "Warning",
                [$"{nameof(AppSettings.Logging)}:ConsoleOnly"] = "Error",
            })
            .Build();

        LoggingSettings? settings = configuration.GetSection(nameof(AppSettings.Logging)).Get<LoggingSettings>();

        Assert.That(settings, Is.Not.Null);
        Assert.That(settings!.Visualization, Is.EqualTo(LogEventLevel.Warning));
        Assert.That(settings.ConsoleOnly, Is.EqualTo(LogEventLevel.Error));
    }
}
