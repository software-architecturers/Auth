using System;
using System.IO;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Template.WebApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .UseSerilog(LoggerConfiguration);

        private static void LoggerConfiguration(WebHostBuilderContext host,
            LoggerConfiguration configuration)
        {
            const string template =
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
            const string logDirectory = "logs";
            var env = host.HostingEnvironment;
            var logPath = Path.Combine(logDirectory, $"{env.ApplicationName}.log");
            // in dev mode clear file logs (only files (a file) from the last launch will remain)
            var directory = new DirectoryInfo(logDirectory);
            if (env.IsDevelopment() && directory.Exists)
            {
                foreach (var file in directory.GetFiles())
                {
                    file.Delete();
                }
            }

            configuration.ReadFrom.Configuration(host.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithDemystifiedStackTraces()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code,
                    outputTemplate: template)
                .WriteTo.Async(s => s.File(path: logPath,
                    outputTemplate: template, buffered: env.IsProduction(), rollOnFileSizeLimit: true,
                    rollingInterval: RollingInterval.Day, retainedFileCountLimit: 31
                ));
        }
    }
}