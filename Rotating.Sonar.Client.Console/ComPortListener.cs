namespace Rotating.Sonar.ClientApp.Console;

using System;
using System.IO.Ports;

public class ComPortListener
{
    public ComPortListener(string portName, int portBaudRate)
    {
        this.portName = portName;
        this.portBaudRate = portBaudRate;
    }

    private readonly string portName;

    private readonly int portBaudRate;

    public void Listen(Func<bool> isCancelled, Action<string>? newLineCallBack = null)
    {
        //TODO cancellation token instead
        //TODO, think of protection and make it a single shot one

        using var serialPort = new SerialPort(this.portName)
        {
            BaudRate = this.portBaudRate,
            DataBits = 8,
            Parity = Parity.None,
            StopBits = StopBits.One,
        };

        try
        {
            serialPort.Open();
            Console.WriteLine($"Port {serialPort.PortName} opened successfully at {serialPort.BaudRate} baud.");
            Console.WriteLine("Reading data from port... (Press any key to stop)");//TODO, get rid of a button
            Console.WriteLine("----------------------------------------");

            while (!isCancelled())
            {
                var line = serialPort.ReadLine()
                    .TrimEnd('\r');
                Console.WriteLine($"Received line: \"{line}\".");

                newLineCallBack?.Invoke(line);
            }
        }
        finally
        {
            serialPort.Close();
        }
    }
}