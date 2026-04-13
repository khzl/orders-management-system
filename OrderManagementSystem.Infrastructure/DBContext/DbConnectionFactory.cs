using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Data.SqlTypes;
using Microsoft.Data.SqlClient;

namespace OrderManagementSystem.Infrastructure.DBContext
{
    // For Dapper 
    public class DbConnectionFactory
    {
        private readonly IConfiguration _config;

        // public Constructor
        public DbConnectionFactory(IConfiguration config)
        {
            _config = config;
        }

        public IDbConnection CreateConnection()
        {
            var connectionString = _config.GetConnectionString("DefaultConnection");

            return new SqlConnection(connectionString);
        }
    }
}
