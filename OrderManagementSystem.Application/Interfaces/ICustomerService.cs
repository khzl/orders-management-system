using OrderManagementSystem.Application.Commons;
using OrderManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Application.Interfaces
{
    public interface ICustomerService
    {
        // Customer CRUD
        public Task<Result<IEnumerable<Entity_Customer>>> GetAllAsync();
        public Task<Result<Entity_Customer>> GetByIdAsync(int customerId);
        public Task<Result<int>> CreateAsync(Entity_Customer customer, string phone, string phoneType = "Mobile");
        public Task<Result> UpdateAsync(Entity_Customer customer);
        public Task<Result> DeleteAsync(int customerId);

        // Phone Management
        public Task<Result> AddPhoneAsync(int customerId, string phone, string phoneType = "Mobile", bool isPrimary = false);
        public Task<Result> DeletePhoneAsync(int phoneId);
    }
}
