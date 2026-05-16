namespace Rotating.Sonar.ClientApp.Console;

using System;
using Microsoft.Extensions.Configuration;
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
        string contentRoot = AppContext.BaseDirectory;
        string configurationSource = Path.Combine(contentRoot, "appsettings.json");

        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(contentRoot)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .Build();

        bool visualizeMode = args.HasCommandFlag("visualize");
        LoggingSettings loggingSettings = configuration.GetSection(nameof(AppSettings.Logging)).Get<LoggingSettings>()
            ?? new LoggingSettings();
        var minimumLogLevel = visualizeMode ? loggingSettings.Visualization : loggingSettings.ConsoleOnly;

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .MinimumLevel.Is(minimumLogLevel)
            .CreateLogger();

        if (args.Length == 0)
        {
            Log.Information("No command line arguments were provided.");
        }
        else
        {
            Log.Information("Command line arguments ({Count}): {Arguments}", args.Length, string.Join(' ', args));
        }

        string loggingMode = visualizeMode ? "visualization" : "console-only";
        var configuredLevel = visualizeMode ? loggingSettings.Visualization : loggingSettings.ConsoleOnly;
        Log.Information("Loading logging configuration from {AppSettingsPath}", configurationSource);
        Log.Information(
            "Logging mode: {Mode}; configured level '{ConfiguredLevel}'; effective minimum level {MinimumLevel}",
            loggingMode,
            configuredLevel,
            minimumLogLevel);

        Log.Information("Scan Display Client Console");
        Log.Information("=============================");
        Log.Information("Desktop client to visualize range/angle data from sonar or lidar-style sensors");
        Log.Information("");

        AppSettings appSettings;
        try
        {
            Log.Information("Loading application settings from {AppSettingsPath}", configurationSource);
            appSettings = AppSettingsConfiguration.BindAndValidate(configuration);
            Log.Information("Application settings loaded and validated successfully from {AppSettingsPath}", configurationSource);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Application settings validation failed for {AppSettingsPath}", configurationSource);
            throw;
        }

        try
        {
            try
            {
                args.ValidateCommandOptions();
            }
            catch (ArgumentException ex)
            {
                Log.Error(ex, "Command line validation failed");
                throw;
            }

            if (visualizeMode)
            {
                Log.Information("Visualization mode enabled: range/angle data will be shown in the scan display window.");
            }

            try
            {
                var regex = appSettings.Serial.CreateLineRegex();

                var comPortListener = CreateComPortListener(args, appSettings.Serial, visualizeMode);

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
                Log.Error(ex, "Serial listener or visualizer failed");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Startup failed");

            LogHelp(appSettings.Serial);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static IComPortListener CreateComPortListener(string[] args, SerialSettings serialSettings, bool visualizeMode)
    {
        string? targetPort = args.GetCommandOption("port");
        string? baudRateStr = args.GetCommandOption("rate");
        bool fakeDataMode = args.HasCommandFlag("fake");

        Log.Information(
            "Resolved connection options: port={Port}, rate={Rate}, fake={FakeData}, visualize={Visualize}",
            targetPort ?? "(not set)",
            baudRateStr ?? "(not set)",
            fakeDataMode,
            visualizeMode);

        if (string.Equals(targetPort, "help", StringComparison.OrdinalIgnoreCase))
        {
            Log.Information("Help was requested (for example -port help).");
            DisplayAvailablePorts();
            LogHelp(serialSettings);
            throw new ArgumentException("Help was requested.");
        }

        if (!fakeDataMode && string.IsNullOrEmpty(targetPort))
        {
            Log.Error("Required command line arguments are missing. Specify -port <name> or use -fake.");
            DisplayAvailablePorts();
            LogHelp(serialSettings);
            throw new ArgumentException("Arguments are not specified.");
        }

        IComPortListener result;

        if (fakeDataMode)
        {
            Log.Information("Fake data mode enabled; using generated serial lines (no COM port required).");
            result = new FakeComPortListener(serialSettings.FakeData);
        }
        else
        {
            int baudRate = serialSettings.DefaultBaudRate;
            if (string.IsNullOrEmpty(baudRateStr))
            {
                Log.Information("Baud rate not specified, using default: {DefaultBaudRate}", serialSettings.DefaultBaudRate);
            }
            else if (!int.TryParse(baudRateStr, out baudRate))
            {
                Log.Warning("Invalid baud rate '{BaudRate}'. Using default: {DefaultBaudRate}", baudRateStr, serialSettings.DefaultBaudRate);
                baudRate = serialSettings.DefaultBaudRate;
            }
            else
            {
                Log.Information("Using baud rate: {BaudRate}", baudRate);
            }

            Log.Information("Connecting to COM port: {TargetPort}", targetPort);

            var portNames = SerialPortProvider.GetPortNames();

            if (!portNames.Contains(targetPort))
            {
                var portNotFound = new ArgumentException($"Port '{targetPort}' not found.");
                Log.Error(portNotFound, "Port '{TargetPort}' not found.", targetPort);
                DisplayAvailablePorts();
                throw portNotFound;
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
