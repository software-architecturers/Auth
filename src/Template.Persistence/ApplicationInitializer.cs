using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Template.Domain.Entities;
using static System.Reflection.BindingFlags;
using ILogger = Serilog.ILogger;

namespace Template.Persistence
{
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class ApplicationInitializer
    {
        private static readonly ILogger Log = Serilog.Log.ForContext<ApplicationInitializer>();

        private ApplicationInitializer()
        {
        }

        public static void Initialize(ApplicationDbContext context)
        {
            if (context.Items.Any())
            {
                return;
            }

            var seedMethods = typeof(ApplicationInitializer)
                .GetMethods(Static | Public | NonPublic)
                .Where(info => info.Name.StartsWith("Seed") &&
                               info.ReturnType == typeof(void) &&
                               info.GetParameters().Length == 1 &&
                               info.GetParameters()[0].ParameterType == typeof(ApplicationDbContext));
            foreach (var seedMethod in seedMethods)
            {
                seedMethod.Invoke(null, new object[] {context});
            }
        }


        private static void SeedItems(ApplicationDbContext context)
        {
            Log.Information("Seeding Items");
            var items = new[]
            {
                new Item {Name = "Item 1", Description = "Test"},
                new Item {Name = "Item 2"},
            };
            context.Items.AddRange(items);
            context.SaveChanges();
        }
    }
}