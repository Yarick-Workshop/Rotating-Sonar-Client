namespace Rotating.Sonar.Client.Visualizer;

using Rotating.Sonar.Client.Common.Settings;
using System;
using Serilog;

public class ScanDisplayVisualizer : IDisposable
{
    private readonly ScanPointBuffer scanPointBuffer = new();
    private ScanDisplayWindow? window;
    private readonly AppSettings appSettings;
    private bool _disposed = false;

    public ScanDisplayVisualizer(AppSettings? appSettings = null)
    {
        this.appSettings = appSettings ?? new AppSettings();
    }

    public void Start(int windowWidthPx = 1920, int windowHeightPx = 1080, string title = "OpenGL Scan Display")
    {
        var win = new ScanDisplayWindow(this.scanPointBuffer, this.appSettings, windowWidthPx, windowHeightPx, title);
        this.window = win;
        win.Run();
    }

    public void FeedData(int angleDeg, int distanceCm)
    {
        Log.Debug("Scan display received point with angle: {Angle}, distance: {Distance}", angleDeg, distanceCm);
        this.scanPointBuffer.Update(angleDeg, distanceCm);
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
