namespace Rotating.Sonar.ClientApp.Console;

using System;
using System.IO.Ports;
using Rotating.Sonar.ClientApp.Console.Extensions;
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using Rotating.Sonar.Client.Visualizer;

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
                // Buffer for incoming lines
                var lineBuffer = new ConcurrentQueue<string>();
                var regex = new Regex(@"(\d+):\s*(\d+)cm", RegexOptions.Compiled);

                var comPortListener = new ComPortListener(targetPort, baudRate);

                if (visualizeMode)
                {
                    using (var visualizer = new PolarPlotVisualizer())
                    using (var cancellationTokenSource = new CancellationTokenSource())
                    {
                        var serialThread = new Thread(() =>
                            comPortListener.Listen(
                                () => cancellationTokenSource.Token.IsCancellationRequested,
                                line => lineBuffer.Enqueue(line)))
                        {
                            IsBackground = true
                        };
                        serialThread.Start();
                        visualizer.Start();

                        while (!cancellationTokenSource.Token.IsCancellationRequested)
                        {
                            while (lineBuffer.TryDequeue(out var line))
                            {
                                Console.WriteLine($"Dequeued \"{line}\".");

                                var match = regex.Match(line);
                                if (match.Success)
                                {
                                    int angle = int.Parse(match.Groups[1].Value);
                                    int distance = int.Parse(match.Groups[2].Value);
                                    visualizer.FeedData(angle, distance);
                                }
                            }
                            if (Console.KeyAvailable)
                            {
                                cancellationTokenSource.Cancel();
                            }
                        }
                    }
                    // TODO: Optionally, stop the visualizer if needed
                }
                else
                {
                    comPortListener.Listen(() => Console.KeyAvailable);
                }
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