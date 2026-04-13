using OrderManagementSystem.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using System.Data;
using OrderManagementSystem.Infrastructure.DBContext;
using OrderManagementSystem.Domain.Entities;

namespace OrderManagementSystem.Infrastructure.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        // private field 
        private readonly DbConnectionFactory _connectionFactory;

        // Constructor  
        public CustomerRepository(DbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        ///  Get All Method
        ///  Get All Customer With Primary Phones Only
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Entity_Customer>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();

            var customers = await connection.QueryAsync<Entity_Customer>(
                "sp_GetCustomers",
                commandType: CommandType.StoredProcedure);

            return customers;
        }

        /// <summary>
        /// Get By Id
        /// SP return result
        /// first -> customer data 
        /// second -> List Phones
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task<Entity_Customer?> GetByIdAsync(int customerId)
        {
            using var connection = _connectionFactory.CreateConnection();

            var multi = await connection.QueryMultipleAsync(
                "sp_GetCustomerById",
                new { CustomerId = customerId },
                commandType: CommandType.StoredProcedure);

            var customer = await multi.ReadFirstOrDefaultAsync<Entity_Customer>();

            if (customer is null)
                return null;

            var phones = await multi.ReadAsync<Entity_CustomerPhones>();
            customer.CustomerPhones = phones.ToList();

            return customer;
        }

        /// <summary>
        /// Add Customer 
        /// SP One in Transaction Add Customer + First Phone Numbers
        /// </summary>
        /// <param name="customer"></param>
        /// <param name="phone"></param>
        /// <param name="phoneType"></param>
        /// <returns></returns>
        public async Task<int> AddAsync(Entity_Customer customer, string phone, string phoneType = "Mobile")
        {
            using var connection = _connectionFactory.CreateConnection();

            var newCustomerId = await connection.ExecuteScalarAsync<int>(
                "sp_AddCustomer",
                new
                {
                    customer.CustomerName,
                    customer.Email,
                    customer.Address,
                    Phone = phone,
                    PhoneType = phoneType
                },
                commandType: CommandType.StoredProcedure);

            return newCustomerId;
        }

        /// <summary>
        ///  Update Customer
        ///  methods customer data only - phones
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public async Task UpdateAsync(Entity_Customer customer)
        {
            using var connection = _connectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "sp_UpdateCustomer",
                new
                {
                    customer.CustomerId,
                    customer.CustomerName,
                    customer.Email,
                    customer.Address
                },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Delete Customer 
        /// Cascade delete phone auto in DB
        /// </summary>
        /// <param name="customerId"></param>
        /// <returns></returns>
        public async Task DeleteAsync(int customerId)
        {
            using var connection = _connectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "sp_DeleteCustomer",
                new { CustomerId = customerId },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Add Phone 
        /// </summary>
        /// <param name="customerId"></param>
        /// <param name="phone"></param>
        /// <param name="phoneType"></param>
        /// <param name="isPrimary"></param>
        /// <returns></returns>
        public async Task AddPhoneAsync(int customerId,string phone,string phoneType = "Mobile", bool isPrimary = false)
        {
            using var connection = _connectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "sp_AddCustomerPhone",
                new
                {
                    CustomerId = customerId,
                    Phone = phone,
                    PhoneType = phoneType,
                    IsPrimary = isPrimary
                },
                commandType: CommandType.StoredProcedure);
        }

        /// <summary>
        /// Delete Phone 
        /// </summary>
        /// <param name="phoneId"></param>
        /// <returns></returns>
        public async Task DeletePhoneAsync(int phoneId)
        {
            using var connection = _connectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "sp_DeleteCustomerPhone",
                new { PhoneId = phoneId },
                commandType: CommandType.StoredProcedure);
        }

    }
}
