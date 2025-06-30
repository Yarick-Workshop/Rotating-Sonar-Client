namespace Rotating.Sonar.ClientApp.Console;

using System;
using System.IO.Ports;
using Rotating.Sonar.ClientApp.Console.Extensions;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Rotating Sonar Client Console");
        Console.WriteLine("=============================");
        Console.WriteLine("Desktop client to visualize data from Rotating-Sonar-Arduino");
        Console.WriteLine();

        // Validate command line arguments
        try
        {
            args.ValidateCommandOptions();
        
            // Parse command line arguments
            string? targetPort = args.GetCommandOption("port");

            // Fetch and display all available COM ports
            var portNames = SerialPort.GetPortNames();
            
            // Show help or list ports if no port specified or help requested
            if (string.IsNullOrEmpty(targetPort) || targetPort.ToLower() == "help")
            {
                DisplayAvailablePorts();
                Console.WriteLine();
                Console.WriteLine("Usage: dotnet run -- -port <port_name>");
                Console.WriteLine("Example: dotnet run -- -port /dev/ttyUSB0");
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
        
            using var serialPort = new SerialPort(targetPort)
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