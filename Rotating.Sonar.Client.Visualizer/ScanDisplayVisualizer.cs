namespace Rotating.Sonar.Client.Visualizer;

using System;
using Rotating.Sonar.Client.Common.Settings;
using Serilog;

public class ScanDisplayVisualizer : IDisposable
{
    private readonly ScanPointBuffer scanPointBuffer = new();
    private ScanDisplayWindow? window;
    private readonly AppSettings appSettings;
    private bool _disposed = false;

    public ScanDisplayVisualizer(AppSettings appSettings)
    {
        ArgumentNullException.ThrowIfNull(appSettings);
        this.appSettings = appSettings;
    }

    public void Start()
    {
        WindowSettings windowSettings = this.appSettings.Visualizer.Window;
        var win = new ScanDisplayWindow(
            this.scanPointBuffer,
            this.appSettings,
            windowSettings.WidthPx,
            windowSettings.HeightPx,
            windowSettings.Title);
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
