using Serilog;
using Serilog.Events;
using Serilog.Formatting.Json;
using Serilog.Sinks.SystemConsole.Themes;

namespace Shop_ProjForWeb.Infrastructure.Logging;

public static class SerilogConfiguration
{
    public static void ConfigureSerilog(IConfiguration configuration)
    {
        var logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "log-.txt");
        var errorLogPath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "error-.txt");
        var seqUrl = configuration["Serilog:SeqUrl"] ?? "http://localhost:5341";
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Environment", environment)
            .Enrich.WithProperty("Application", "Shop_ProjForWeb.API");

        // Console sink with colored output
        loggerConfiguration.WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}",
            theme: AnsiConsoleTheme.Code,
            restrictedToMinimumLevel: LogEventLevel.Information);

        // File sink for all logs (JSON format)
        loggerConfiguration.WriteTo.File(
            path: logPath,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            formatter: new JsonFormatter(),
            shared: true,
            restrictedToMinimumLevel: LogEventLevel.Information);

        // Separate file for errors
        loggerConfiguration.WriteTo.File(
            path: errorLogPath,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 90,
            formatter: new JsonFormatter(),
            shared: true,
            restrictedToMinimumLevel: LogEventLevel.Error);

        // Seq sink (if available)
        if (!string.IsNullOrEmpty(seqUrl))
        {
            try
            {
                loggerConfiguration.WriteTo.Seq(
                    serverUrl: seqUrl,
                    restrictedToMinimumLevel: LogEventLevel.Information);
            }
            catch
            {
                // Seq might not be available, continue without it
            }
        }

        Log.Logger = loggerConfiguration.CreateLogger();
    }
}

