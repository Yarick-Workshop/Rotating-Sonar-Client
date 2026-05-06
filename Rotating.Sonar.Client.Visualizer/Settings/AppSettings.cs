namespace Rotating.Sonar.Client.Visualizer;

using Microsoft.Extensions.Configuration;

public sealed class AppSettings
{
    public VisualizerSettings Visualizer { get; set; } = new();

    public static AppSettings Load(string path)
    {
        try
        {
            string? directory = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            if (string.IsNullOrWhiteSpace(directory) || string.IsNullOrWhiteSpace(fileName))
            {
                return new AppSettings();
            }

            IConfiguration configuration = new ConfigurationBuilder()
                .SetBasePath(directory)
                .AddJsonFile(fileName, optional: true, reloadOnChange: false)
                .Build();

            // FloatColor4 uses [TypeConverter(typeof(FloatColor4TypeConverter))] so string values bind.
            return configuration.Get<AppSettings>() ?? new AppSettings();
        }
        catch
        {
            return new AppSettings();
        }
    }
}
