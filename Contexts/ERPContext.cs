using Microsoft.EntityFrameworkCore;
using WorkerService1.Models;

namespace WorkerService1.Contexts
{
    public class ERPContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        public DbSet<Platform> Platforms { get; set; }

        public ERPContext(DbContextOptions<ERPContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
