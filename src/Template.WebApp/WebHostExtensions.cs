using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Template.Persistence;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Template.WebApp
{
    public static class WebHostExtensions
    {

        private static readonly ILogger Log = Serilog.Log.Logger;
        public static IWebHost SeedDatabase(this IWebHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;
                var env = serviceProvider.GetRequiredService<IHostingEnvironment>();
                if (!env.IsDevelopment())
                {
                    return host;
                }
                
                try
                {
                    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
                    context.Database.Migrate();
                    Log.Information("Seeding the database with data...");
                    ApplicationInitializer.Initialize(context);
                    Log.Information("Seeding the database finished");
                }
                catch (Exception e)
                {
                    Log.Fatal(e, "An error occurred while seeding the database");
                    
                }
            }

            return host;
        }
    }
}