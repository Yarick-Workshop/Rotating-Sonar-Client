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
        var win = new PolarPlotWindow(sonarDataCache, width, height, title);
        window = win;
        win.Run();
    }

    public void FeedData(int angle, int distance)
    {
        Log.Debug($"Plot is fed with angle: {angle}, distance: {distance}");
        sonarDataCache.Update(angle, distance);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        window?.Dispose();
    }
}
