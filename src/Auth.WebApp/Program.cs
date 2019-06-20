using System.IO;
using System.Net;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace Auth.WebApp
{
    public class Program
    {
        public static void Main(string[] args) =>
            CreateWebHostBuilder(args).Build().SeedDatabase().Run();

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) => WebHost
            .CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .UseSerilog(LoggerConfiguration)
            .UseKestrel(options =>
            {
                options.Listen(IPAddress.Any, 5000);
                options.Listen(IPAddress.Any, 5001, listenOptions =>
                {
                    // password is temporary as an empty openssl password causes an issue
                    listenOptions.UseHttps("localhost.pfx", "12345");
                });
            });

        private static void LoggerConfiguration(WebHostBuilderContext host,
            LoggerConfiguration configuration)
        {
            const string template =
                "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}";
          
            var env = host.HostingEnvironment;
           

            configuration.ReadFrom.Configuration(host.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithDemystifiedStackTraces()
                .WriteTo.Console(theme: AnsiConsoleTheme.Code,
                    outputTemplate: template);
            if (env.IsDevelopment())
            {
                const string logDirectory = "Logs";
                var logPath = Path.Combine(logDirectory, $"{env.ApplicationName}.log");
                var directory = new DirectoryInfo(logDirectory);
                if (directory.Exists)
                {
                    foreach (var file in directory.GetFiles())
                    {
                        file.Delete();
                    }
                }
                configuration.WriteTo.Async(s => s.File(path: logPath,
                    outputTemplate: template, buffered: env.IsProduction(), rollOnFileSizeLimit: true,
                    rollingInterval: RollingInterval.Day, retainedFileCountLimit: 31
                ));
            }
        }
    }
}