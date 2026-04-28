using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Infrastructure.DBContext.Configurations
{
    public class CustomerPhoneConfiguration : IEntityTypeConfiguration<Entity_CustomerPhones>
    {
        public void Configure(EntityTypeBuilder<Entity_CustomerPhones> builder)
        {
            builder.ToTable("CustomerPhones");

            builder.HasKey(cp => cp.PhoneId);

            builder.Property(cp => cp.PhoneNumber)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(cp => cp.PhoneType)
                .HasMaxLength(20);

            builder.Property(cp => cp.IsPrimary)
                .IsRequired()
                .HasDefaultValue(false);

            // index on Customer Id For Run JOIN
            builder.HasIndex(cp => cp.CustomerId)
                .HasDatabaseName("IX_CustomerPhones_CustomerId");

            // filtered Unique Index not be more in IsPrimary=true for same Customer 
            builder.HasIndex(cp => new
            {
                cp.CustomerId,
                cp.IsPrimary
            })
                .IsUnique()
                .HasFilter("[IsPrimary] = 1")
                .HasDatabaseName("UQ_CustomerPhones_OnePrimary");
        }

    }
}
