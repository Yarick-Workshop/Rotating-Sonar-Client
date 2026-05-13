namespace Rotating.Sonar.Client.Common.SerialPorts.Listeners;

using System;
using System.IO.Ports;
using Serilog;

public class ComPortListener : IComPortListener
{
    private readonly int portBaudRate;
    private readonly int readTimeoutMilliseconds;

    public string PortName { get; }

    public ComPortListener(string portName, int portBaudRate, int readTimeoutMilliseconds)
    {
        this.PortName = portName;
        this.portBaudRate = portBaudRate;
        this.readTimeoutMilliseconds = readTimeoutMilliseconds;
    }

    public void Listen(Func<bool> isCancelled, Action<string>? newLineCallBack = null)
    {
        using var serialPort = new SerialPort(this.PortName)
        {
            BaudRate = this.portBaudRate,
            DataBits = 8,
            Parity = Parity.None,
            StopBits = StopBits.One,
            ReadTimeout = this.readTimeoutMilliseconds,
        };

        try
        {
            serialPort.Open();

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