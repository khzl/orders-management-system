using OrderManagementSystem.Application.Commons;
using OrderManagementSystem.Application.Interfaces;
using OrderManagementSystem.Domain.Entities;
using OrderManagementSystem.Infrastructure.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace OrderManagementSystem.Application.Services
{
    public class CustomerService : ICustomerService 
    {
        // private field 
        private readonly ICustomerRepository _customerRepo;

        // Constructor Injection
        public CustomerService(ICustomerRepository customerRepo)
        {
            _customerRepo = customerRepo;
        }

        // Get All Customer 
        public async Task<Result<IEnumerable<Entity_Customer>>> GetAllAsync()
        {
            var customers = await _customerRepo.GetAllAsync();
            return Result<IEnumerable<Entity_Customer>>.Sucess(customers);
        }

        // Get Customer By Id
        public async Task<Result<Entity_Customer>> GetByIdAsync(int customerId)
        {
            if (customerId <= 0)
                return Result<Entity_Customer>.Failure("Invalid customer Id");

            var customer = await _customerRepo.GetByIdAsync(customerId);

            if (customer is null)
                return Result<Entity_Customer>.Failure($"Customer {customerId} Not Found");

            return Result<Entity_Customer>.Sucess(customer);
        }

        //  Create Customer 
        public async Task<Result<int>> CreateAsync(Entity_Customer customer,string phone,string phoneType = "Mobile")
        {
            var customerValidation = Validations.ValidateCustomer(customer);
            if (!customerValidation.IsSuccess)
                return Result<int>.Failure(customerValidation.Error ?? "Customer validation failed");

            var phoneValidation = Validations.ValidatePhone(phone);
            if (!phoneValidation.IsSuccess)
                return Result<int>.Failure(phoneValidation.Error ?? "Phone validation failed");

            var newCustomerId = await _customerRepo.AddAsync(customer, phone, phoneType);
            return Result<int>.Sucess(newCustomerId);
        }

        // Update Customer
        public async Task<Result> UpdateAsync(Entity_Customer customer)
        {
            if (customer.CustomerId <= 0)
                return Result.Failure("Invalid Customer Id");

            var validation = Validations.ValidateCustomer(customer);
            if (!validation.IsSuccess)
                return validation;

            await _customerRepo.UpdateAsync(customer);
            return Result.Success();
        }

        // Delete Customer 
        public async Task<Result> DeleteAsync(int customerId)
        {
            if (customerId <= 0)
                return Result.Failure("Invalid Customer Id");

            await _customerRepo.DeleteAsync(customerId);
            return Result.Success();
        }

        // Add Phone 
        public async Task<Result> AddPhoneAsync(
            int customerId,
            string phone,
            string phoneType = "Mobile",
            bool isPrimary = false)
        {
            if (customerId <= 0)
                return Result.Failure("Invalid Customer Id");

            var validation = Validations.ValidatePhone(phone);
            if (!validation.IsSuccess)
                return validation;

            await _customerRepo.AddPhoneAsync(customerId, phone, phoneType, isPrimary);
            return Result.Success();
        }

        // Delete Phone 
        public async Task<Result> DeletePhoneAsync(int phoneId)
        {
            if (phoneId <= 0)
                return Result.Failure("Invalid Phone Id");

            await _customerRepo.DeletePhoneAsync(phoneId);
            return Result.Success();
        }

    }
}
