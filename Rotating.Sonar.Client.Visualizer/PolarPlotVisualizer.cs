namespace Rotating.Sonar.Client.Visualizer;

using System;
using Serilog;

public class PolarPlotVisualizer : IDisposable
{
    private readonly SonarDataCache sonarDataCache = new();
    private PolarPlotWindow? window;
    private bool _disposed = false;

    public void Start(int width = 1920, int height = 1080, string title = "OpenGL Polar Plot Visualizer")
    {
        var win = new PolarPlotWindow(this.sonarDataCache, width, height, title);
        this.window = win;
        win.Run();
    }

    public void FeedData(int angle, int distance)
    {
        Log.Debug($"Plot is fed with angle: {angle}, distance: {distance}");
        this.sonarDataCache.Update(angle, distance);
    }

    public void Dispose()
    {
        if (this._disposed) return;
        this._disposed = true;
        this.window?.Dispose();
    }
}
