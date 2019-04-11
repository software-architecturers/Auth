using Microsoft.EntityFrameworkCore;
using Template.Domain.Entities;
using Template.Persistence.Configurations;

namespace Template.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Item> Items { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ItemConfiguration).Assembly);
        }
    }
}