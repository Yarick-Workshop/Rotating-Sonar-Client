namespace Rotating.Sonar.Client.Visualizer;

using Rotating.Sonar.Client.Common.Settings;
using System;
using Serilog;

public class PolarPlotVisualizer : IDisposable
{
    private readonly SonarDataCache sonarDataCache = new();
    private PolarPlotWindow? window;
    private readonly AppSettings appSettings;
    private bool _disposed = false;

    public PolarPlotVisualizer(AppSettings? appSettings = null)
    {
        this.appSettings = appSettings ?? new AppSettings();
    }

    public void Start(int windowWidthPx = 1920, int windowHeightPx = 1080, string title = "OpenGL Polar Plot Visualizer")
    {
        var win = new PolarPlotWindow(this.sonarDataCache, this.appSettings, windowWidthPx, windowHeightPx, title);
        this.window = win;
        win.Run();
    }

    public void FeedData(int angleDeg, int distanceCm)
    {
        Log.Debug("Plot is fed with angle: {Angle}, distance: {Distance}", angleDeg, distanceCm);
        this.sonarDataCache.Update(angleDeg, distanceCm);
    }

    public void Dispose()
    {
        if (this._disposed)
        {
            return;
        }

        this._disposed = true;
        this.window?.Dispose();
    }
}
