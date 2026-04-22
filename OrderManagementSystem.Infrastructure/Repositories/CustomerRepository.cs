using OrderManagementSystem.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using System.Data;
using OrderManagementSystem.Infrastructure.DBContext;
using OrderManagementSystem.Domain.Entities;
using System.Data.Common;

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

            // Dictionary For Save Customers and منع التكرار اثناء القراءة 
            var customerDictionary = new Dictionary<int, Entity_Customer>();

            var customers = await connection.QueryAsync<Entity_Customer, Entity_CustomerPhones, Entity_Customer>(
                "sp_GetCustomers",
                (customer, phone) =>
                {
                    if (!customerDictionary.TryGetValue(customer.CustomerId, out var currentCustomer))
                    {
                        currentCustomer = customer;
                        currentCustomer.CustomerPhones = new List<Entity_CustomerPhones>();
                        customerDictionary.Add(currentCustomer.CustomerId, currentCustomer);
                    }

                    if (phone != null)
                    {
                        currentCustomer.CustomerPhones.Add(phone);
                    }

                    return currentCustomer;
                },
                splitOn: "PhoneId", // يخبر Dapper أين ينتهي العميل ويبدأ الهاتف
                commandType: CommandType.StoredProcedure);

            return customerDictionary.Values;
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
        ///  Add With Phones Async
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public async Task<int> AddWithPhonesAsync(Entity_Customer customer)
        {
            // Cast لـ DbConnection لأن IDbConnection ما عنده OpenAsync
            using var connection = (DbConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Add Customer and Fetch ID 
                var customerId = await connection.ExecuteScalarAsync<int>(
                    "sp_AddCustomerBasic",
                    new
                    {
                        customer.CustomerName,
                        customer.Email,
                        customer.Address
                    },
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure);

                // Add Phones 
                if (customer.CustomerPhones != null && customer.CustomerPhones.Any())
                {
                    const string sql = @"INSERT INTO CustomerPhones
                                         (CustomerId,PhoneNumber,PhoneType,IsPrimary)
                                         VALUES (@CustomerId,@PhoneNumber,@PhoneType,@IsPrimary)";

                    // نحول لـ List مرة وحدة بدل ما نحسب First() في كل iteration
                    var phones = customer.CustomerPhones.ToList();

                    for (int index = 0; index < phones.Count; index++)
                    {
                        await connection.ExecuteAsync(sql, new
                        {
                            CustomerId = customerId,
                            phones[index].PhoneNumber,
                            phones[index].PhoneType,
                            IsPrimary = index == 0 // First Number Is Primary
                        },
                        transaction: transaction);
                    }
                }

                // Commit 
                await transaction.CommitAsync();
                return customerId;
            }
            catch
            {
                // تحقق قبل الـ Rollback لأن الـ connection ممكن يكون انقطع
                if (connection.State == ConnectionState.Open)
                    await transaction.RollbackAsync();

                throw;
            }
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
        /// Delete All Customers  
        /// </summary>
        /// <returns></returns>
        public async Task DeleteAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();

            await connection.ExecuteAsync(
                "sp_DeleteAllCustomers",
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
