namespace Rotating.Sonar.ClientApp.Console;

using System;
using System.IO.Ports;
using Rotating.Sonar.ClientApp.Console.Extensions;

class Program
{
    private const int DEFAULT_BAUD_RATE = 9600; // Standard Arduino speed

    static void Main(string[] args)
    {
        Console.WriteLine("Rotating Sonar Client Console");
        Console.WriteLine("=============================");
        Console.WriteLine("Desktop client to visualize data from Rotating-Sonar-Arduino");
        Console.WriteLine();

        try
        {
            args.ValidateCommandOptions();

            // Parse command line arguments
            string? targetPort = args.GetCommandOption("port");
            string? baudRateStr = args.GetCommandOption("rate");
            bool visualizeMode = args.HasCommandFlag("visualize");

            // Parse baud rate
            int baudRate = DEFAULT_BAUD_RATE; // Default baud rate
            if (string.IsNullOrEmpty(baudRateStr))
            {
                Console.WriteLine($"Baud rate not specified, using default: {DEFAULT_BAUD_RATE}");
            }
            else
            {
                if (!int.TryParse(baudRateStr, out baudRate))
                {
                    Console.WriteLine($"Error: Invalid baud rate '{baudRateStr}'. Using default: {DEFAULT_BAUD_RATE}");
                    baudRate = DEFAULT_BAUD_RATE;
                }
                else
                {
                    Console.WriteLine($"Using baud rate: {baudRate}");
                }
            }

            // Print visualization info if requested
            if (visualizeMode)
            {
                Console.WriteLine("Visualization mode enabled: This would visualize sonar data if OpenGL support was present.");
            }

            // Fetch and display all available COM ports
            var portNames = SerialPort.GetPortNames();
            
            // Show help or list ports if no port specified or help requested
            if (string.IsNullOrEmpty(targetPort) || targetPort.ToLower() == "help")
            {
                DisplayAvailablePorts();
                Console.WriteLine();
                Console.WriteLine("Usage: dotnet run -- -port <port_name> [-rate <baud_rate>] [-visualize]");
                Console.WriteLine("Parameters:");
                Console.WriteLine("  -port <port_name>    COM port to connect to (required)");
                Console.WriteLine($"  -rate <baud_rate>    Baud rate (optional, default: {DEFAULT_BAUD_RATE})");
                Console.WriteLine("  -visualize           Enable visualization of sonar data (optional)");
                Console.WriteLine();
                Console.WriteLine("Examples:");
                Console.WriteLine("  dotnet run -- -port /dev/ttyUSB0");
                Console.WriteLine("  dotnet run -- -port /dev/ttyUSB0 -rate 115200");
                Console.WriteLine("  dotnet run -- -port /dev/ttyUSB0 -visualize");
                Console.WriteLine("  dotnet run -- -port /dev/ttyUSB0 -rate 115200 -visualize");
                return;
            }

            // Check if the specified port exists
            if (!portNames.Contains(targetPort))
            {
                Console.WriteLine($"Error: Port '{targetPort}' not found.");
                DisplayAvailablePorts();
                return;
            }

            // Open the specified port and read data
            Console.WriteLine($"Opening port: {targetPort}");
        
            try
            {
                using var serialPort = new SerialPort(targetPort)
                {
                    BaudRate = baudRate,
                    DataBits = 8,
                    Parity = Parity.None,
                    StopBits = StopBits.One,
                };

                serialPort.DataReceived += (sender, e) =>
                {
                    if (e.EventType == SerialData.Chars)
                    {
                        Console.Write(serialPort.ReadExisting());
                    }
                };

                serialPort.Open();
                Console.WriteLine($"Port {targetPort} opened successfully at {serialPort.BaudRate} baud.");
                Console.WriteLine("Reading data from port... (Press any key to stop)");
                Console.WriteLine("----------------------------------------");

                // Keep the application running until a key is pressed
                Console.ReadKey();

                serialPort.Close();
                Console.WriteLine("\nPort closed.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening port {targetPort}: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }

    /// <summary>
    /// Displays all available COM ports to the console
    /// </summary>
    static void DisplayAvailablePorts()
    {
        var portNames = SerialPort.GetPortNames();
        
        if (portNames.Length == 0)
        {
            Console.WriteLine("No COM ports found.");
        }
        else
        {
            Console.WriteLine("Available COM ports:");
            foreach (var portName in portNames)
            {
                Console.WriteLine($"- {portName}");
            }
        }
    }
} 