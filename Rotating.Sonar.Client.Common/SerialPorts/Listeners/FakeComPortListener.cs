namespace Rotating.Sonar.Client.Common.SerialPorts.Listeners;

using System;
using Serilog;

public class FakeComPortListener : IComPortListener
{
    public string PortName => "FAKE";

    public void Listen(Func<bool> isCancelled, Action<string>? newLineCallBack = null)
    {
        //TODO, to configuration???
        int min = -90, max = 90;
        int currentAngle = min, step = 15, distance = 120;

        var rnd = new Random();

        while (!isCancelled())
        {
            var line = $"{currentAngle}: {distance + rnd.Next(-10, 11)}cm"; // TODO, to config file?

            Log.Debug("Received line: \"{Line}\".", line);

            newLineCallBack?.Invoke(line);

            currentAngle += step;

            if (currentAngle >= max || currentAngle <= min)
            {
                step = -step;
            }

            Thread.Sleep(100);
        }
    }
}