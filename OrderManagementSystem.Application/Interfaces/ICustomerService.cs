using OrderManagementSystem.Application.Commons;
using OrderManagementSystem.Domain.Entities;
using OrderManagementSystem.Dtos.Customers;
using OrderManagementSystem.Shared;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Application.Interfaces
{
    public interface ICustomerService
    {
        // Customer CRUD
        public Task<Result<PaginationResult<CustomerDto>>> GetAllAsync(int pageNumber,int pageSize);
        public Task<Result<CustomerDto>> GetByIdAsync(int customerId);
        public Task<Result<int>> CreateAsync(CreateCustomerDto createCustomerDto);
        public Task<Result> UpdateAsync(UpdateCustomerDto updateCustomerDto);
        public Task<Result> DeleteAsync(int customerId);
        public Task<Result> DeleteAllAsync();

        // Phone Management
        public Task<Result<CustomerPhoneDto>> AddPhoneAsync(CustomerPhoneDto customerPhoneDto);
        public Task<Result> DeletePhoneAsync(int phoneId);
    }
}
