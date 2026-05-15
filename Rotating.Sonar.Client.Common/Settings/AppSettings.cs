namespace Rotating.Sonar.Client.Common.Settings;

using Microsoft.Extensions.Configuration;
using Serilog;

public sealed class AppSettings
{
    public SerialSettings Serial { get; set; } = new();

    public VisualizerSettings Visualizer { get; set; } = new();

    public static AppSettings Load(string path)
    {
        var settings = LoadInternal(path);

        SettingsValidator.ValidateRecursively(settings, nameof(AppSettings));

        return settings;
    }

    private static AppSettings LoadInternal(string path)
    {
        try
        {
            string? directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(fileName))
            {
                Log.Warning("App settings path is invalid: {Path}. Falling back to defaults.", path);
                return new AppSettings();
            }

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(directory)
                .AddJsonFile(fileName, optional: true, reloadOnChange: false)
                .Build();

            // FloatColor4 uses [TypeConverter(typeof(FloatColor4TypeConverter))] so string values bind.
            AppSettings? appSettings = configuration.Get<AppSettings>();
            if (appSettings is null)
            {
                Log.Warning("Failed to bind app settings from {Path}. Falling back to defaults.", path);
                return new AppSettings();
            }

            return appSettings;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while loading app settings from {Path}. Falling back to defaults.", path);
            return new AppSettings();
        }
    }
}
