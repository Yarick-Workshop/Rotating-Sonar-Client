namespace Rotating.Sonar.Client.Visualizer;

using OpenTK.Windowing.Desktop;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;

public class PolarPlotVisualizer
{
    private readonly PolarPlotData plotData = new();
    private PolarPlotWindow? window;

    public void Start(int width = 600, int height = 600, string title = "OpenGL Polar Plot Visualizer")
    {
        var nativeWinSettings = new NativeWindowSettings()
        {
            APIVersion = new Version(3, 2),
            ClientSize = new Vector2i(width, height),
            Title = title,
            Flags = ContextFlags.Default,
        };

        using var win = new PolarPlotWindow(GameWindowSettings.Default, nativeWinSettings, plotData);
        window = win;
        win.Run();
    }

    public void FeedData(int angle, int distance)
    {
        Console.WriteLine($"Plot is fed with angle: {angle}, distance: {distance}");
        plotData.Update(angle, distance);
    }
}
