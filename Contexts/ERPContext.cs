using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
