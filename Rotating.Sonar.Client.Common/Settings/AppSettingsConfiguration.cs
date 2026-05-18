namespace Rotating.Sonar.Client.Common.Settings;

using Microsoft.Extensions.Configuration;

public static class AppSettingsConfiguration
{
    public static AppSettings BindAndValidate(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        AppSettings settings = configuration.Get<AppSettings>() ?? new AppSettings();
        SettingsValidator.ValidateRecursively(settings, nameof(AppSettings));
        return settings;
    }
}
