namespace Rotating.Sonar.ClientApp.Console;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rotating.Sonar.Client.Common.Settings;
using Rotating.Sonar.ClientApp.Console.Extensions;
using Serilog;

internal class ConsoleHost
{
    public static void Main(string[] args)
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

        try
        {
            using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
                builder.AddSerilog(Log.Logger, dispose: false));

            ILogger<ConsoleHost> hostLogger = loggerFactory.CreateLogger<ConsoleHost>();

            AppSettings appSettings;
            try
            {
                hostLogger.LogInformation("Loading application settings from {AppSettingsPath}", configurationSource);
                appSettings = AppSettingsConfiguration.BindAndValidate(configuration);
                hostLogger.LogInformation("Application settings loaded and validated successfully from {AppSettingsPath}", configurationSource);
            }
            catch (Exception ex)
            {
                hostLogger.LogError(ex, "Application settings validation failed for {AppSettingsPath}", configurationSource);
                throw;
            }

            var app = new ConsoleApplication(
                loggerFactory.CreateLogger<ConsoleApplication>(),
                loggerFactory);

            app.LogStartupBanner(loggingSettings, visualizeMode, configurationSource, minimumLogLevel);
            app.Run(args, appSettings, visualizeMode);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}
