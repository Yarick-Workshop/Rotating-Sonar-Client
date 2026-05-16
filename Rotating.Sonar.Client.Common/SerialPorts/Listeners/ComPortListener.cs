namespace Rotating.Sonar.Client.Common.SerialPorts.Listeners;

using System;
using System.IO.Ports;
using Microsoft.Extensions.Logging;

public class ComPortListener : IComPortListener
{
    private readonly int portBaudRate;
    private readonly int readTimeoutMilliseconds;
    private readonly ILogger<ComPortListener> logger;

    public string PortName { get; }

    public ComPortListener(string portName, int portBaudRate, int readTimeoutMilliseconds, ILogger<ComPortListener> logger)
    {
        this.PortName = portName;
        this.portBaudRate = portBaudRate;
        this.readTimeoutMilliseconds = readTimeoutMilliseconds;
        this.logger = logger;
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

                    this.logger.LogDebug("Received line: \"{Line}\".", line);

                    newLineCallBack?.Invoke(line);
                }
                catch (TimeoutException)
                {
                    this.logger.LogInformation("Serial port read timed out while waiting for data.");
                }
            }
        }
        finally
        {
            serialPort.Close();
        }
    }
}
