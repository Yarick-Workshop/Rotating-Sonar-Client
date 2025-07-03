namespace Rotating.Sonar.ClientApp.Console;

using System;
using System.IO.Ports;
using Serilog;

public class ComPortListener : IComPortListener
{
    public string PortName { get; }

    private readonly int portBaudRate;

    public ComPortListener(string portName, int portBaudRate)
    {
        this.PortName = portName;
        this.portBaudRate = portBaudRate;
    }

    public void Listen(Func<bool> isCancelled, Action<string>? newLineCallBack = null)
    {
        using var serialPort = new SerialPort(this.PortName)
        {
            BaudRate = this.portBaudRate,
            DataBits = 8,
            Parity = Parity.None,
            StopBits = StopBits.One,
        };

        try
        {
            serialPort.Open();
            Log.Information($"Port {this.PortName} opened successfully at {serialPort.BaudRate} baud.");
            Log.Information("Reading data from port... (Press any key to stop)");//TODO, get rid of a button
            Log.Information("----------------------------------------");

            while (!isCancelled())
            {
                var line = serialPort.ReadLine()
                    .TrimEnd('\r');
                Log.Debug($"Received line: \"{line}\".");

                newLineCallBack?.Invoke(line);
            }
        }
        finally
        {
            serialPort.Close();
        }
    }
}