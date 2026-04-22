using OrderManagementSystem.Application.Commons;
using OrderManagementSystem.Domain.Entities;
using OrderManagementSystem.Dtos.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Application.Interfaces
{
    public interface ICustomerService
    {
        // Customer CRUD
        public Task<Result<IEnumerable<CustomerDto>>> GetAllAsync();
        public Task<Result<CustomerDto>> GetByIdAsync(int customerId);
        public Task<Result<int>> CreateAsync(CreateCustomerDto createCustomerDto);
        public Task<Result<int>> CreateWithPhonesAsync(CreateCustomerDto createCustomerDto);
        public Task<Result> UpdateAsync(UpdateCustomerDto updateCustomerDto);
        public Task<Result> DeleteAsync(int customerId);
        public Task<Result> DeleteAllAsync();

        // Phone Management
        public Task<Result> AddPhoneAsync(int customerId, string phone, string phoneType = "Mobile", bool isPrimary = false);
        public Task<Result> DeletePhoneAsync(int phoneId);
    }
}
