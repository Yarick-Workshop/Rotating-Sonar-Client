namespace Rotating.Sonar.Client.Visualizer;

using System;

public class PolarPlotVisualizer : IDisposable
{
    private readonly PolarPlotData plotData = new();
    private PolarPlotWindow? window;
    private bool _disposed = false;

    public void Start(int width = 600, int height = 600, string title = "OpenGL Polar Plot Visualizer")
    {
        var win = new PolarPlotWindow(plotData);
        window = win;
        win.Run(width, height, title);
    }

    public void FeedData(int angle, int distance)
    {
        Console.WriteLine($"Plot is fed with angle: {angle}, distance: {distance}");
        plotData.Update(angle, distance);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        window?.Dispose();
    }
}
