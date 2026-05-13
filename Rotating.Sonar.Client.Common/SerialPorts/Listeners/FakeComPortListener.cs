namespace Rotating.Sonar.Client.Common.SerialPorts.Listeners;

using Rotating.Sonar.Client.Common.Settings;
using System;
using Serilog;

public class FakeComPortListener : IComPortListener
{
    private readonly FakeSerialDataSettings fakeDataSettings;

    public string PortName => "FAKE";

    public FakeComPortListener(FakeSerialDataSettings fakeDataSettings)
    {
        this.fakeDataSettings = fakeDataSettings;
    }

    public void Listen(Func<bool> isCancelled, Action<string>? newLineCallBack = null)
    {
        int min = this.fakeDataSettings.MinAngleDeg;
        int max = this.fakeDataSettings.MaxAngleDeg;
        int currentAngle = min;
        int step = this.fakeDataSettings.AngleStepDeg;
        int distance = this.fakeDataSettings.BaseDistanceCm;
        int jitter = this.fakeDataSettings.DistanceJitterCm;
        int intervalMilliseconds = this.fakeDataSettings.IntervalMilliseconds;

        var rnd = new Random();

        while (!isCancelled())
        {
            var line = $"{currentAngle}: {distance + rnd.Next(-jitter, jitter + 1)}cm";

            Log.Debug("Received line: \"{Line}\".", line);

            newLineCallBack?.Invoke(line);

            currentAngle += step;

            if (currentAngle >= max || currentAngle <= min)
            {
                step = -step;
            }

            Thread.Sleep(intervalMilliseconds);
        }
    }
}