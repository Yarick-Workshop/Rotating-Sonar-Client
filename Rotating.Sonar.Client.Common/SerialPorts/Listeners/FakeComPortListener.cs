namespace Rotating.Sonar.Client.Common.SerialPorts.Listeners;

using Rotating.Sonar.Client.Common.Settings;
using System;
using Serilog;

public class FakeComPortListener : IComPortListener
{
    private readonly FakeSerialDataSettings fakeData;

    public string PortName => "FAKE";

    public FakeComPortListener(FakeSerialDataSettings fakeData)
    {
        this.fakeData = fakeData;
    }

    public void Listen(Func<bool> isCancelled, Action<string>? newLineCallBack = null)
    {
        int min = this.fakeData.MinAngleDeg;
        int max = this.fakeData.MaxAngleDeg;
        int currentAngle = min;
        int step = this.fakeData.AngleStepDeg;
        int distance = this.fakeData.BaseDistanceCm;
        int jitter = this.fakeData.DistanceJitterCm;
        int intervalMilliseconds = this.fakeData.IntervalMilliseconds;

        var rnd = new Random();
        var lineFormatter = new FakeSerialLineFormatter(this.fakeData.FakeSerialLineFormat);

        while (!isCancelled())
        {
            int distanceCm = distance + rnd.Next(-jitter, jitter + 1);
            var line = lineFormatter.CreateLine(currentAngle, distanceCm);

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
