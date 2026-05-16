using OrderManagementSystem.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Dapper;
using System.Data;
using OrderManagementSystem.Infrastructure.DBContext;
using OrderManagementSystem.Domain.Entities;
using System.Data.Common;
using OrderManagementSystem.Shared;

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
        public async Task<PaginationResult<Entity_Customer>> GetAllAsync(int pageNumber,int pageSize)
        {
            using var connection = _connectionFactory.CreateConnection();

            // Used QueryMultiple 
            var result = await connection.QueryMultipleAsync(
                "sp_GetCustomers",
                new
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize
                },
                commandType: CommandType.StoredProcedure);

            var customers = 
                (await result.ReadAsync<Entity_Customer>()).ToList();

            var phones = 
                (await result.ReadAsync<Entity_CustomerPhones>()).ToList();

            var totalCount =
                await result.ReadFirstAsync<int>();

            // linked Phones to Customers Using Dictionary For Fast Lookup
            var customerDictionary = 
                customers.ToDictionary(c => c.CustomerId);

            foreach(var phone in phones)
            {
                if (customerDictionary.TryGetValue
                    (phone.CustomerId, 
                    out var customer))
                {
                    customer.CustomerPhones.Add(phone);
                }
            }

            return new PaginationResult<Entity_Customer>
            {
                Data = customers,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount
            };
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
            var phones = await multi.ReadAsync<Entity_CustomerPhones>();

            customer?.CustomerPhones = phones.ToList();

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
        public async Task<int> AddAsync(Entity_Customer customer)
        {
            using var connection = _connectionFactory.CreateConnection();

            var newCustomerId = await connection.ExecuteScalarAsync<int>(
                "sp_AddCustomer",
                new
                {
                    customer.CustomerName,
                    customer.Email,
                    customer.Address,
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
            using var connection = (DbConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // 1- Add Customer 
                var customerId = await connection.ExecuteScalarAsync<int>(
                    "sp_AddCustomer",
                    new
                    {
                        customer.CustomerName,
                        customer.Email,
                        customer.Address
                    },
                    transaction: transaction,
                    commandType: CommandType.StoredProcedure);

                // 2- Add Phones Using SP
                if (customer.CustomerPhones != null && customer.CustomerPhones.Any())
                {
                    foreach (var phone in customer.CustomerPhones)
                    {
                        await connection.ExecuteAsync(
                            "sp_AddCustomerPhone",
                            new
                            {
                                CustomerId = customerId,
                                Phone = phone.PhoneNumber,
                                PhoneType = phone.PhoneType,
                                IsPrimary = phone.IsPrimary
                            },
                            transaction: transaction,
                            commandType: CommandType.StoredProcedure);
                    }
                }
                await transaction.CommitAsync();
                return customerId;
            }
            catch
            {
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

            connection.Open();

            using var transaction = connection.BeginTransaction();

            try
            {
                // Update Customer 
                await connection.ExecuteAsync(
                    "sp_UpdateCustomer",
                    new
                    {
                        customer.CustomerId,
                        customer.CustomerName,
                        customer.Email,
                        customer.Address
                    },
                    transaction,
                    commandType: CommandType.StoredProcedure);

                // Delete Phones 
                foreach (var phoneId in customer.DeletedPhoneIds ?? new List<int>())
                {
                    await connection.ExecuteAsync(
                        "sp_DeleteCustomerPhone",
                        new
                        {
                            PhoneId = phoneId
                        },
                        transaction,
                        commandType: CommandType.StoredProcedure);
                }

                // Add Or Update Phones
                foreach (var phone in customer.CustomerPhones ?? new List<Entity_CustomerPhones>())
                {
                    if (phone.PhoneId > 0)
                    {
                        // Update Phone 
                        await connection.ExecuteAsync(
                            "sp_UpdateCustomerPhone",
                            new
                            {
                                phone.PhoneId,
                                phone.PhoneNumber,
                                phone.PhoneType,
                                phone.IsPrimary
                            },
                            transaction,
                            commandType: CommandType.StoredProcedure);
                    }
                    else
                    {
                        // Add Phone
                        await connection.ExecuteAsync(
                            "sp_AddCustomerPhone",
                            new
                            {
                                customer.CustomerId,
                                phone.PhoneNumber,
                                phone.PhoneType,
                                phone.IsPrimary
                            },
                            transaction,
                            commandType: CommandType.StoredProcedure);
                    }
                }
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
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
