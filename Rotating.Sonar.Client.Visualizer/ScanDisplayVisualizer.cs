namespace Rotating.Sonar.Client.Visualizer;

using System;
using Microsoft.Extensions.Logging;
using Rotating.Sonar.Client.Common.Settings;
using Rotating.Sonar.Client.Common.Settings.Sections;

public class ScanDisplayVisualizer : IDisposable
{
    private readonly ScanPointBuffer scanPointBuffer;
    private readonly ILoggerFactory loggerFactory;
    private ScanDisplayWindow? window;
    private readonly AppSettings appSettings;
    private readonly ILogger<ScanDisplayVisualizer> logger;
    private bool disposed = false;

    public ScanDisplayVisualizer(
        AppSettings appSettings,
        ILogger<ScanDisplayVisualizer> logger,
        ILoggerFactory loggerFactory)
    {
        ArgumentNullException.ThrowIfNull(appSettings);
        this.appSettings = appSettings;
        this.logger = logger;
        this.loggerFactory = loggerFactory;
        this.scanPointBuffer = new ScanPointBuffer(loggerFactory.CreateLogger<ScanPointBuffer>());
    }

    public void Start()
    {
        WindowSettings windowSettings = this.appSettings.Visualizer.Window;
        var win = new ScanDisplayWindow(
            this.scanPointBuffer,
            this.appSettings,
            windowSettings.WidthPx,
            windowSettings.HeightPx,
            windowSettings.Title,
            this.loggerFactory.CreateLogger<ScanDisplayWindow>(),
            this.loggerFactory.CreateLogger<OpenGlScanDisplayRenderer>());
        this.window = win;
        win.Run();
    }

    public void FeedData(int angleDeg, int distanceCm)
    {
        this.logger.LogDebug("Scan display received point with angle: {Angle}, distance: {Distance}", angleDeg, distanceCm);
        this.scanPointBuffer.Update(angleDeg, distanceCm);
    }

    public void Dispose()
    {
        if (this.disposed)
        {
            return;
        }

        this.disposed = true;
        this.window?.Dispose();
    }
}
