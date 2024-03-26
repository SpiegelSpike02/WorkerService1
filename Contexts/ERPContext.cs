using Microsoft.EntityFrameworkCore;
using WorkerService1.Models;

namespace WorkerService1.Contexts
{
    public class ERPContext(DbContextOptions<ERPContext> options) : DbContext(options)
    {
        public DbSet<Product> Products { get; set; }

        public DbSet<Platform> Platforms { get; set; }

        public DbSet<hhyy> hhyys { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
