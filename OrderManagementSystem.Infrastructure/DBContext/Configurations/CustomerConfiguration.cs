using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Infrastructure.DBContext.Configurations
{
    public class CustomerConfiguration : IEntityTypeConfiguration<Entity_Customer>
    {
        public void Configure(EntityTypeBuilder<Entity_Customer> builder)
        {
            builder.ToTable("Customers");

            builder.HasKey(c => c.CustomerId);

            builder.Property(c => c.CustomerName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(c => c.Address)
                .HasMaxLength(250);

            builder.Property(c => c.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(c => c.UpdatedAt)
                .IsRequired(false);

            // Unique Index On Email
            builder.HasIndex(c => c.Email)
                .IsUnique()
                .HasDatabaseName("UQ_Customers_Email");

            // Index for Search Name 
            builder.HasIndex(c => c.CustomerName)
                .HasDatabaseName("IX_Customers_Name");

            // Relationship 
            builder.HasMany(c => c.CustomerPhones)
                .WithOne()
                .HasForeignKey(c => c.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

        }
    }
}
