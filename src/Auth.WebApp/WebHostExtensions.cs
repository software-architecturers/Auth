using System;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Auth.Domain.Entities;
using Auth.Persistence;
using IHostingEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;

namespace Auth.WebApp
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
                
                try
                {
                    var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
                    Log.Information("Seeding the database with data...");
                    context.Database.Migrate();
                    ApplicationInitializer.Initialize(context);
                    
                    ApplicationInitializer.SeedUsers(context, serviceProvider.GetRequiredService<UserManager<ApplicationUser>>());
                    scope.ServiceProvider.GetService<PersistedGrantDbContext>().Database.Migrate();
                    var configurationDbContext = scope.ServiceProvider.GetService<ConfigurationDbContext>();
                    configurationDbContext.Database.Migrate();
                    ApplicationInitializer.SeedConfiguration(configurationDbContext);
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