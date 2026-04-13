using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using OrderManagementSystem.Domain.Entities;

namespace OrderManagementSystem.Infrastructure.DBContext
{
    // For Entity Framework Core
    public class AppDbContext : DbContext
    {
        // Constructor 
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // DbSet Define for Entities
        public DbSet<Entity_Customer> Customers { get; set; }
        public DbSet<Entity_CustomerPhones> CustomerPhones { get; set; }

        // here Registers Configuration
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // All Configuration Auto Register from same assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

    }
}
