using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderManagementSystem.Infrastructure.DBContext;
using OrderManagementSystem.Infrastructure.Interfaces;
using OrderManagementSystem.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace OrderManagementSystem.Infrastructure
{
    // DI For Data Access
    public static class DataStartUp
    {
        public static IServiceCollection AddDataLayer(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // DbContext Registration for Entity Framework Core 
            // Register DbContext In DI
            services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Dapper Registration for raw SQL queries and stored procedures
            services.AddScoped<IDbConnection>(sp =>
            new SqlConnection(configuration.GetConnectionString("DefaultConnection")));

            // Register Factory like Singleton because need Config only and no changed 
            services.AddSingleton<DbConnectionFactory>();

            // Dapper Repositories registration
            services.AddScoped<ICustomerRepository, CustomerRepository>();

            // return the services collection so the extension can be chained 
            return services;
        }
    }
}
