namespace Rotating.Sonar.Client.Common.ComPortListeners;

using System;
using System.IO.Ports;
using Serilog;

public class ComPortListener : IComPortListener
{
    private const int ReadTimeoutMilliseconds = 250;

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
            ReadTimeout = ReadTimeoutMilliseconds,
        };

        try
        {
            serialPort.Open();
            Log.Information("Port {PortName} opened successfully at {BaudRate} baud.", this.PortName, serialPort.BaudRate);
            Log.Information("Reading data from port... (Press any key to stop)");//TODO, get rid of a button
            Log.Information("----------------------------------------");

            while (!isCancelled())
            {
                try
                {
                    var line = serialPort.ReadLine()
                        .TrimEnd('\r');
                    Log.Debug("Received line: \"{Line}\".", line);

                    newLineCallBack?.Invoke(line);
                }
                catch (TimeoutException)
                {
                    Log.Information("Serial port read timed out while waiting for data.");
                }
            }
        }
        finally
        {
            serialPort.Close();
        }
    }
}