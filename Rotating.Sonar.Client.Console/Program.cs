namespace Rotating.Sonar.ClientApp.Console;

using Rotating.Sonar.Client.Common.Settings;
using System;
using Rotating.Sonar.Client.Common.ComPortListeners;
using Rotating.Sonar.Client.Common.SerialPorts;
using Rotating.Sonar.ClientApp.Console.Extensions;
using System.Text.RegularExpressions;
using Rotating.Sonar.Client.Visualizer;
using Serilog;
using System.Diagnostics;

class Program
{
    private const int DEFAULT_BAUD_RATE = 9600; // Standard Arduino speed

    static void Main(string[] args)
    {
        if (args.Length == 0 && Debugger.IsAttached)
        {
            args = ["-fake", "-visualize"]; // For testing purposes, remove in production

            Log.Warning("No command line arguments provided, using default test arguments: -fake -visualize");
        }

        Log.Logger = new LoggerConfiguration()
            // TODO, uncomment to see debug logs .MinimumLevel.Debug()
            .WriteTo.Async(a => a.Console())
            .CreateLogger();
        
        Log.Information("Scan Display Client Console");
        Log.Information("=============================");
        Log.Information("Desktop client to visualize range/angle data from sonar or lidar-style sensors");
        Log.Information("");

        try
        {
            args.ValidateCommandOptions();

            bool visualizeMode = args.HasCommandFlag("visualize");

            // Print visualization info if requested
            if (visualizeMode)
            {
                Log.Information("Visualization mode enabled: range/angle data will be shown in the scan display window.");
            }

            try
            {
                var regex = new Regex(@"([+-]?\d+):\s*(\d+)cm", RegexOptions.Compiled);

                var comPortListener = CreateComPortListener(args);
                
                if (visualizeMode)
                {
                    var appSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                    var appSettings = AppSettings.Load(appSettingsPath);
                    using var visualizer = new ScanDisplayVisualizer(appSettings);
                    using var cancellationTokenSource = new CancellationTokenSource();

                    var serialThread = new Thread(() =>
                        comPortListener.Listen(
                            () => cancellationTokenSource.Token.IsCancellationRequested,
                            line =>
                            {
                                var match = regex.Match(line);
                                if (match.Success)
                                {
                                    int angle = int.Parse(match.Groups[1].Value);
                                    int distance = int.Parse(match.Groups[2].Value);
                                    visualizer.FeedData(angle, distance);
                                }
                            }))
                    {
                        IsBackground = true
                    };
                    serialThread.Start();
                    try
                    {
                        visualizer.Start();
                    }
                    finally
                    {
                        cancellationTokenSource.Cancel();
                        serialThread.Join();
                    }
                }
                else
                {
                    comPortListener.Listen(() => Console.KeyAvailable);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error: {ErrorMessage}", ex.Message);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error: {ErrorMessage}", ex.Message);

            LogHelp();
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static IComPortListener CreateComPortListener(string[] args)
    {
        // Parse command line arguments
        string? targetPort = args.GetCommandOption("port");
        string? baudRateStr = args.GetCommandOption("rate");
        bool fakeDataMode = args.HasCommandFlag("fake");

        if ((!fakeDataMode && string.IsNullOrEmpty(targetPort)) || targetPort?.ToLower() == "help")
        {
            DisplayAvailablePorts();
            LogHelp();
            throw new ArgumentException("Arguments are not specified.");
        }

        IComPortListener result;

        if (fakeDataMode)
        {
            result = new FakeComPortListener();
        }
        else
        {
            // Parse baud rate
            int baudRate = DEFAULT_BAUD_RATE; // Default baud rate
            if (string.IsNullOrEmpty(baudRateStr))
            {
                Log.Information("Baud rate not specified, using default: {DefaultBaudRate}", DEFAULT_BAUD_RATE);
            }
            else
            {
                if (!int.TryParse(baudRateStr, out baudRate))
                {
                    Log.Warning("Invalid baud rate '{BaudRate}'. Using default: {DefaultBaudRate}", baudRateStr, DEFAULT_BAUD_RATE);
                    baudRate = DEFAULT_BAUD_RATE;
                }
                else
                {
                    Log.Information("Using baud rate: {BaudRate}", baudRate);
                }
            }

            // Fetch and display all available COM ports
            var portNames = SerialPortProvider.GetPortNames();

            // Check if the specified port exists
            if (!portNames.Contains(targetPort))
            {
                Log.Error("Port '{TargetPort}' not found.", targetPort);
                DisplayAvailablePorts();
                throw new ArgumentException($"Port '{targetPort}' not found.");
            }

            result = new ComPortListener(targetPort!, baudRate);
        }

        // Open the created port and read data
        Log.Information("Opened port: {PortName}.", result.PortName);

        return result;
    }

    private static void LogHelp()
    {
        Log.Information("");
        Log.Information("Usage: dotnet run -- -port <port_name> [-rate <baud_rate>] [-visualize]");
        Log.Information("Parameters:");
        Log.Information("  -port <port_name>    COM port to connect to (required)");
        Log.Information("  -rate <baud_rate>    Baud rate (optional, default: {DefaultBaudRate})", DEFAULT_BAUD_RATE);
        Log.Information("  -visualize           Enable scan display visualization (optional)");
        Log.Information("  -fake                Generate fake randomize COM port data. A fake com port (optional). If the parameter is set no real COM port configuration is needed");
        Log.Information("");
        Log.Information("Examples:");
        Log.Information("  dotnet run -- -port /dev/ttyUSB0");
        Log.Information("  dotnet run -- -port /dev/ttyUSB0 -rate 115200");
        Log.Information("  dotnet run -- -port /dev/ttyUSB0 -visualize");
        Log.Information("  dotnet run -- -port /dev/ttyUSB0 -rate 115200 -visualize");
        Log.Information("  dotnet run -- -fake");
        Log.Information("  dotnet run -- -fake -visualize");
    }

    /// <summary>
    /// Displays all available COM ports to the console
    /// </summary>
    static void DisplayAvailablePorts()
    {
        var portNames = SerialPortProvider.GetPortNames();
        
        if (portNames.Length == 0)
        {
            Log.Warning("No COM ports found.");
        }
        else
        {
            Log.Information("Available COM ports:");
            foreach (var portName in portNames)
            {
                Log.Information("- {PortName}", portName);
            }
        }
    }
} 