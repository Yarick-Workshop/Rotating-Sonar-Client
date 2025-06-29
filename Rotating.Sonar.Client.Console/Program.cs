namespace Rotating.Sonar.ClientApp.Console;

using System;
using System.IO.Ports;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Rotating Sonar Client Console");
        Console.WriteLine("=============================");
        Console.WriteLine("Desktop client to visualize data from Rotating-Sonar-Arduino");
        Console.WriteLine();

        // Fetch and display all available COM ports
        var portNames = SerialPort.GetPortNames();
        if (portNames.Length == 0)
        {
            Console.WriteLine("No COM ports found.");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
            return;
        }

        Console.WriteLine("Available COM ports:");
        foreach (var portName in portNames)
        {
            Console.WriteLine($"- {portName}");
        }
        Console.WriteLine();

        // Open the first available port and read data
        var firstPortName = portNames.First();
        Console.WriteLine($"Opening port: {firstPortName}");
        
        try
        {
            using var serialPort = new SerialPort(firstPortName)
            {
                BaudRate = 9600,  // Standard Arduino speed
                DataBits = 8,
                Parity = Parity.None,
                StopBits = StopBits.One,
            };

            serialPort.DataReceived += (sender, e) =>
            {
                if (e.EventType == SerialData.Chars)
                {
                    try
                    {
                        string data = serialPort.ReadExisting();
                        if (!string.IsNullOrEmpty(data))
                        {
                            Console.Write(data);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error reading data: {ex.Message}");
                    }
                }
            };

            serialPort.Open();
            Console.WriteLine($"Port {firstPortName} opened successfully at {serialPort.BaudRate} baud.");
            Console.WriteLine("Reading data from port... (Press any key to stop)");
            Console.WriteLine("----------------------------------------");

            // Keep the application running until a key is pressed
            Console.ReadKey();

            serialPort.Close();
            Console.WriteLine("\nPort closed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error opening port {firstPortName}: {ex.Message}");
        }

        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
} 