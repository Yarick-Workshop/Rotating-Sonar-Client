namespace Rotating.Sonar.Client.Tests.Settings;

using Microsoft.Extensions.Configuration;
using Rotating.Sonar.Client.Common.Settings;
using Serilog.Events;

[TestFixture]
public sealed class AppSettingsConfiguration_Tests
{
    [Test]
    public void BindAndValidate_NullConfiguration_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            AppSettingsConfiguration.BindAndValidate(null!));
    }

    [Test]
    public void BindAndValidate_EmptyConfiguration_ReturnsAppSettingsWithValidDefaults()
    {
        IConfiguration configuration = new ConfigurationBuilder().Build();

        AppSettings settings = AppSettingsConfiguration.BindAndValidate(configuration);

        Assert.That(settings, Is.Not.Null);
        Assert.DoesNotThrow(() =>
            SettingsValidator.ValidateRecursively(settings, nameof(AppSettings)));
    }

    [Test]
    public void BindAndValidate_InvalidLoggingLevel_ThrowsInvalidOperationException()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{nameof(AppSettings.Logging)}:Visualization"] = "999",
            })
            .Build();

        Assert.Throws<InvalidOperationException>(() =>
            AppSettingsConfiguration.BindAndValidate(configuration));
    }
}
