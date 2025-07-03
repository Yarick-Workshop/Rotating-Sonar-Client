namespace Rotating.Sonar.ClientApp.Console;

using System;
using Serilog;
using Silk.NET.GLFW;

public class FakeComPortListener : IComPortListener
{
    public string PortName => "FAKE";

    public void Listen(Func<bool> isCancelled, Action<string>? newLineCallBack = null)
    {
        Log.Information($"Port wit FAKE data \"opened\" successfully.");
        Log.Information("(Press any key to stop)");//TODO, get rid of a button
        Log.Information("----------------------------------------");

        //TODO, to configuration???
        int min = -90, max = 90;
        int currentAngle = min, step = 15, distance = 120;

        var rnd = new Random();

        while (!isCancelled())
        {
            var line = $"{currentAngle}: {distance + rnd.Next(-10, 11)}cm"; // TODO, to config file?

            Console.WriteLine(line);

            Log.Debug($"Received line: \"{line}\".");

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