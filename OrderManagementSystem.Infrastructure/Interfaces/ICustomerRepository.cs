using OrderManagementSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace OrderManagementSystem.Infrastructure.Interfaces
{
    public interface ICustomerRepository
    {
        // Contracts 

        // Customer CRUD
        public Task<IEnumerable<Entity_Customer>> GetAllAsync();
        public Task<Entity_Customer?> GetByIdAsync(int customerId);
        public Task<int> AddAsync(Entity_Customer customer);
        public Task<int> AddWithPhonesAsync(Entity_Customer customer);
        public Task UpdateAsync(Entity_Customer customer);
        public Task DeleteAsync(int customerId);
        public Task DeleteAllAsync();

        // Phone Management 
        public Task AddPhoneAsync(int customerId, string phone, string phoneType = "Mobile", bool isPrimary = false);
        public Task DeletePhoneAsync(int phoneId);
    }
}
