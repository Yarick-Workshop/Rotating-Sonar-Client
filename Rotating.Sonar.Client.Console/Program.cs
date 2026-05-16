namespace Rotating.Sonar.ClientApp.Console;

using System;
using System.Diagnostics;
using Rotating.Sonar.Client.Common.SerialPorts;
using Rotating.Sonar.Client.Common.SerialPorts.Listeners;
using Rotating.Sonar.Client.Common.Settings;
using Rotating.Sonar.Client.Visualizer;
using Rotating.Sonar.ClientApp.Console.Extensions;
using Serilog;

class Program
{
    static void Main(string[] args)
    {
        if (args.Length == 0 && Debugger.IsAttached)
        {
            args = ["-fake", "-visualize"];

            Log.Warning("No command line arguments provided, using default test arguments: -fake -visualize");
        }

        Log.Logger = new LoggerConfiguration()
            .WriteTo.Async(a => a.Console())
            .CreateLogger();

        Log.Information("Scan Display Client Console");
        Log.Information("=============================");
        Log.Information("Desktop client to visualize range/angle data from sonar or lidar-style sensors");
        Log.Information("");

        var appSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        var appSettings = AppSettings.Load(appSettingsPath);

        try
        {
            args.ValidateCommandOptions();

            bool visualizeMode = args.HasCommandFlag("visualize");

            if (visualizeMode)
            {
                Log.Information("Visualization mode enabled: range/angle data will be shown in the scan display window.");
            }

            try
            {
                var regex = appSettings.Serial.CreateLineRegex();

                var comPortListener = CreateComPortListener(args, appSettings.Serial);

                if (visualizeMode)
                {
                    using var visualizer = new ScanDisplayVisualizer(appSettings);
                    using var cancellationTokenSource = new CancellationTokenSource();

                    Log.Information("Reading data from port: {PortName}.", comPortListener.PortName);
                    Log.Information("----------------------------------------");

                    var serialThread = new Thread(() =>
                        comPortListener.Listen(
                            () => cancellationTokenSource.Token.IsCancellationRequested,
                            line =>
                            {
                                var match = regex.Match(line);
                                if (match.Success)
                                {
                                    int angle = int.Parse(match.Groups[SerialLineParseConstants.AngleGroupName].Value);
                                    int distance = int.Parse(match.Groups[SerialLineParseConstants.DistanceGroupName].Value);
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
                    Log.Information("Reading data from port: {PortName}. (Press any key to stop)", comPortListener.PortName);
                    Log.Information("----------------------------------------");
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

            LogHelp(appSettings.Serial);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static IComPortListener CreateComPortListener(string[] args, SerialSettings serialSettings)
    {
        string? targetPort = args.GetCommandOption("port");
        string? baudRateStr = args.GetCommandOption("rate");
        bool fakeDataMode = args.HasCommandFlag("fake");

        if ((!fakeDataMode && string.IsNullOrEmpty(targetPort)) || targetPort?.ToLower() == "help")
        {
            DisplayAvailablePorts();
            LogHelp(serialSettings);
            throw new ArgumentException("Arguments are not specified.");
        }

        IComPortListener result;

        if (fakeDataMode)
        {
            result = new FakeComPortListener(serialSettings.FakeData);
        }
        else
        {
            int baudRate = serialSettings.DefaultBaudRate;
            if (string.IsNullOrEmpty(baudRateStr))
            {
                Log.Information("Baud rate not specified, using default: {DefaultBaudRate}", serialSettings.DefaultBaudRate);
            }
            else
            {
                if (!int.TryParse(baudRateStr, out baudRate))
                {
                    Log.Warning("Invalid baud rate '{BaudRate}'. Using default: {DefaultBaudRate}", baudRateStr, serialSettings.DefaultBaudRate);
                    baudRate = serialSettings.DefaultBaudRate;
                }
                else
                {
                    Log.Information("Using baud rate: {BaudRate}", baudRate);
                }
            }

            var portNames = SerialPortProvider.GetPortNames();

            if (!portNames.Contains(targetPort))
            {
                Log.Error("Port '{TargetPort}' not found.", targetPort);
                DisplayAvailablePorts();
                throw new ArgumentException($"Port '{targetPort}' not found.");
            }

            result = new ComPortListener(targetPort!, baudRate, serialSettings.ReadTimeoutMilliseconds);
        }

        Log.Information("Opened port: {PortName}.", result.PortName);

        return result;
    }

    private static void LogHelp(SerialSettings serialSettings)
    {
        Log.Information("");
        Log.Information("Usage: dotnet run -- -port <port_name> [-rate <baud_rate>] [-visualize]");
        Log.Information("Parameters:");
        Log.Information("  -port <port_name>    COM port to connect to (required)");
        Log.Information("  -rate <baud_rate>    Baud rate (optional, default: {DefaultBaudRate})", serialSettings.DefaultBaudRate);
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
