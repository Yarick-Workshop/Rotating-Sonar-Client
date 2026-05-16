namespace Rotating.Sonar.ClientApp.Console;

using System;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Rotating.Sonar.Client.Common.SerialPorts;
using Rotating.Sonar.Client.Common.SerialPorts.Listeners;
using Rotating.Sonar.Client.Common.Settings;
using Rotating.Sonar.Client.Visualizer;
using Rotating.Sonar.ClientApp.Console.Extensions;
using Serilog.Events;

public class ConsoleApplication
{
    private readonly ILogger<ConsoleApplication> logger;
    private readonly ILoggerFactory loggerFactory;

    public ConsoleApplication(ILogger<ConsoleApplication> logger, ILoggerFactory loggerFactory)
    {
        this.logger = logger;
        this.loggerFactory = loggerFactory;
    }

    public void Run(string[] args, AppSettings appSettings, bool visualizeMode)
    {
        if (args.Length == 0)
        {
            this.logger.LogInformation("No command line arguments were provided.");
        }
        else
        {
            this.logger.LogInformation("Command line arguments ({Count}): {Arguments}", args.Length, string.Join(' ', args));
        }

        try
        {
            try
            {
                args.ValidateCommandOptions();
            }
            catch (ArgumentException ex)
            {
                this.logger.LogError(ex, "Command line validation failed");
                throw;
            }

            if (visualizeMode)
            {
                this.logger.LogInformation("Visualization mode enabled: range/angle data will be shown in the scan display window.");
            }

            try
            {
                Regex regex = appSettings.Serial.CreateLineRegex();

                IComPortListener comPortListener = this.CreateComPortListener(args, appSettings.Serial, visualizeMode);

                if (visualizeMode)
                {
                    using var visualizer = new ScanDisplayVisualizer(
                        appSettings,
                        this.loggerFactory.CreateLogger<ScanDisplayVisualizer>(),
                        this.loggerFactory);
                    using var cancellationTokenSource = new CancellationTokenSource();

                    this.logger.LogInformation("Reading data from port: {PortName}.", comPortListener.PortName);
                    this.logger.LogInformation("----------------------------------------");

                    var serialThread = new Thread(() =>
                        comPortListener.Listen(
                            () => cancellationTokenSource.Token.IsCancellationRequested,
                            line =>
                            {
                                Match match = regex.Match(line);
                                if (match.Success)
                                {
                                    int angle = int.Parse(match.Groups[SerialLineParseConstants.AngleGroupName].Value);
                                    int distance = int.Parse(match.Groups[SerialLineParseConstants.DistanceGroupName].Value);
                                    visualizer.FeedData(angle, distance);
                                }
                            }))
                    {
                        IsBackground = true,
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
                    this.logger.LogInformation("Reading data from port: {PortName}. (Press any key to stop)", comPortListener.PortName);
                    this.logger.LogInformation("----------------------------------------");
                    comPortListener.Listen(() => Console.KeyAvailable);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "Serial listener or visualizer failed");
            }
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Startup failed");

            this.LogHelp(appSettings.Serial);
        }
    }

    public void LogStartupBanner(LoggingSettings loggingSettings, bool visualizeMode, string configurationSource, LogEventLevel minimumLogLevel)
    {
        string loggingMode = visualizeMode ? "visualization" : "console-only";
        var configuredLevel = visualizeMode ? loggingSettings.Visualization : loggingSettings.ConsoleOnly;
        this.logger.LogInformation("Loading logging configuration from {AppSettingsPath}", configurationSource);
        this.logger.LogInformation(
            "Logging mode: {Mode}; configured level '{ConfiguredLevel}'; effective minimum level {MinimumLevel}",
            loggingMode,
            configuredLevel,
            minimumLogLevel);

        this.logger.LogInformation("Scan Display Client Console");
        this.logger.LogInformation("=============================");
        this.logger.LogInformation("Desktop client to visualize range/angle data from sonar or lidar-style sensors");
        this.logger.LogInformation("");
    }

    private IComPortListener CreateComPortListener(string[] args, SerialSettings serialSettings, bool visualizeMode)
    {
        string? targetPort = args.GetCommandOption("port");
        string? baudRateStr = args.GetCommandOption("rate");
        bool fakeDataMode = args.HasCommandFlag("fake");

        this.logger.LogInformation(
            "Resolved connection options: port={Port}, rate={Rate}, fake={FakeData}, visualize={Visualize}",
            targetPort ?? "(not set)",
            baudRateStr ?? "(not set)",
            fakeDataMode,
            visualizeMode);

        if (string.Equals(targetPort, "help", StringComparison.OrdinalIgnoreCase))
        {
            this.logger.LogInformation("Help was requested (for example -port help).");
            this.DisplayAvailablePorts();
            this.LogHelp(serialSettings);
            throw new ArgumentException("Help was requested.");
        }

        if (!fakeDataMode && string.IsNullOrEmpty(targetPort))
        {
            this.logger.LogError("Required command line arguments are missing. Specify -port <name> or use -fake.");
            this.DisplayAvailablePorts();
            this.LogHelp(serialSettings);
            throw new ArgumentException("Arguments are not specified.");
        }

        IComPortListener result;

        if (fakeDataMode)
        {
            this.logger.LogInformation("Fake data mode enabled; using generated serial lines (no COM port required).");
            result = new FakeComPortListener(
                serialSettings.FakeData,
                this.loggerFactory.CreateLogger<FakeComPortListener>());
        }
        else
        {
            int baudRate = serialSettings.DefaultBaudRate;
            if (string.IsNullOrEmpty(baudRateStr))
            {
                this.logger.LogInformation("Baud rate not specified, using default: {DefaultBaudRate}", serialSettings.DefaultBaudRate);
            }
            else if (!int.TryParse(baudRateStr, out baudRate))
            {
                this.logger.LogWarning("Invalid baud rate '{BaudRate}'. Using default: {DefaultBaudRate}", baudRateStr, serialSettings.DefaultBaudRate);
                baudRate = serialSettings.DefaultBaudRate;
            }
            else
            {
                this.logger.LogInformation("Using baud rate: {BaudRate}", baudRate);
            }

            this.logger.LogInformation("Connecting to COM port: {TargetPort}", targetPort);

            var portNames = SerialPortProvider.GetPortNames();

            if (!portNames.Contains(targetPort))
            {
                var portNotFound = new ArgumentException($"Port '{targetPort}' not found.");
                this.logger.LogError(portNotFound, "Port '{TargetPort}' not found.", targetPort);
                this.DisplayAvailablePorts();
                throw portNotFound;
            }

            result = new ComPortListener(
                targetPort!,
                baudRate,
                serialSettings.ReadTimeoutMilliseconds,
                this.loggerFactory.CreateLogger<ComPortListener>());
        }

        this.logger.LogInformation("Opened port: {PortName}.", result.PortName);

        return result;
    }

    private void LogHelp(SerialSettings serialSettings)
    {
        this.logger.LogInformation("");
        this.logger.LogInformation("Usage: dotnet run -- -port <port_name> [-rate <baud_rate>] [-visualize]");
        this.logger.LogInformation("Parameters:");
        this.logger.LogInformation("  -port <port_name>    COM port to connect to (required)");
        this.logger.LogInformation("  -rate <baud_rate>    Baud rate (optional, default: {DefaultBaudRate})", serialSettings.DefaultBaudRate);
        this.logger.LogInformation("  -visualize           Enable scan display visualization (optional)");
        this.logger.LogInformation("  -fake                Generate fake randomize COM port data. A fake com port (optional). If the parameter is set no real COM port configuration is needed");
        this.logger.LogInformation("");
        this.logger.LogInformation("Examples:");
        this.logger.LogInformation("  dotnet run -- -port /dev/ttyUSB0");
        this.logger.LogInformation("  dotnet run -- -port /dev/ttyUSB0 -rate 115200");
        this.logger.LogInformation("  dotnet run -- -port /dev/ttyUSB0 -visualize");
        this.logger.LogInformation("  dotnet run -- -port /dev/ttyUSB0 -rate 115200 -visualize");
        this.logger.LogInformation("  dotnet run -- -fake");
        this.logger.LogInformation("  dotnet run -- -fake -visualize");
    }

    private void DisplayAvailablePorts()
    {
        var portNames = SerialPortProvider.GetPortNames();

        if (portNames.Length == 0)
        {
            this.logger.LogWarning("No COM ports found.");
        }
        else
        {
            this.logger.LogInformation("Available COM ports:");
            foreach (var portName in portNames)
            {
                this.logger.LogInformation("- {PortName}", portName);
            }
        }
    }
}
