namespace Rotating.Sonar.Client.Common.Settings;

using Rotating.Sonar.Client.Common.Settings.Sections;

public sealed class AppSettings
{
    public SerialSettings Serial { get; set; } = new();

    public VisualizerSettings Visualizer { get; set; } = new();

    public LoggingSettings Logging { get; set; } = new();
}
