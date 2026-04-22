using Microsoft.Data.SqlClient;
using OrderManagementSystem.Application.Commons;
using OrderManagementSystem.Application.Interfaces;
using OrderManagementSystem.Application.Mapper;
using OrderManagementSystem.Domain.Entities;
using OrderManagementSystem.Dtos.Customers;
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
        public async Task<Result<IEnumerable<CustomerDto>>> GetAllAsync()
        {
            var customerEntity = await _customerRepo.GetAllAsync();
            var dtos = customerEntity.Select(CustomerMapper.ToDto).ToList();
            return Result<IEnumerable<CustomerDto>>.Sucess(dtos);
        }

        // Get Customer By Id
        public async Task<Result<CustomerDto>> GetByIdAsync(int customerId)
        {
            if (customerId <= 0)
                return Result<CustomerDto>.Failure("Invalid customer Id");

            var customerEntity = await _customerRepo.GetByIdAsync(customerId);

            if (customerEntity is null)
                return Result<CustomerDto>.Failure($"Customer {customerId} Not Found");

            return Result<CustomerDto>.Sucess(CustomerMapper.ToDto(customerEntity));
        }

        //  Create Customer 
        public async Task<Result<int>> CreateAsync(CreateCustomerDto createCustomerDto)
        {
            // Map to the single-customer validation API (existing signature expects a CustomerDto)
            var customerForValidation = new CustomerDto
            {
                CustomerName = createCustomerDto.CustomerName,
                Email = createCustomerDto.Email,
                Address = createCustomerDto.Address
            };

            var customerValidation = Validations.ValidateCustomer(customerForValidation);
            if (!customerValidation.IsSuccess)
                return Result<int>.Failure(customerValidation.Error ?? "Customer validation failed");

            // Validation Phones 
            if (createCustomerDto.CustomerPhones == null || createCustomerDto.CustomerPhones.Count == 0)
                return Result<int>.Failure("At least One Phone Number Is Required");

            foreach (var phone in createCustomerDto.CustomerPhones) 
            {
                if (string.IsNullOrWhiteSpace(phone?.PhoneNumber))
                    return Result<int>.Failure("Phone number is required.");

                var phoneValidation = Validations.ValidatePhone(phone.PhoneNumber);
                if (!phoneValidation.IsSuccess)
                    return Result<int>.Failure($"Phone '{phone.PhoneNumber}' is invalid: {phoneValidation.Error}");
            }

            try
            {
                var customerEntity = CustomerMapper.ToEntity(createCustomerDto);
                // Repository expects phones or single phone; use AddWithPhonesAsync because we validated phones above
                var newCustomerId = await _customerRepo.AddWithPhonesAsync(customerEntity);
                return Result<int>.Sucess(newCustomerId);
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                return Result<int>.Failure("This Email Is already Registered..");
            }
            catch (Exception ex)
            {
                return Result<int>.Failure($"Unexpected Error: {ex.Message}");
            }
        }

        // Create With Phones Async 
        public async Task<Result<int>> CreateWithPhonesAsync(CreateCustomerDto createCustomerDto)
        {
            // 1. Validate Customer - map Entity_Customer to CustomerDto (existing validation signature)
            var customerForValidation = new CustomerDto
            {
                CustomerName = createCustomerDto.CustomerName,
                Email = createCustomerDto.Email,
                Address = createCustomerDto.Address
            };

            var customerValidation = Validations.ValidateCustomer(customerForValidation);
            if (!customerValidation.IsSuccess)
                return Result<int>.Failure(customerValidation.Error ?? "Customer validation failed");

            // 2. Validate Phones
            if (createCustomerDto.CustomerPhones == null || createCustomerDto.CustomerPhones.Count == 0)
                return Result<int>.Failure("At least one phone number is required.");

            foreach (var phone in createCustomerDto.CustomerPhones)
            {
                if (string.IsNullOrWhiteSpace(phone.PhoneNumber))
                    return Result<int>.Failure("Phone number is required.");

                var phoneValidation = Validations.ValidatePhone(phone.PhoneNumber);
                if (!phoneValidation.IsSuccess)
                    return Result<int>.Failure(
                        $"Phone '{phone.PhoneNumber}' is invalid: {phoneValidation.Error}");
            }

            try
            {
                // Map DTO to entity before calling repository (fixes CS1503)
                var customerEntity = CustomerMapper.ToEntity(createCustomerDto);
                var newCustomerId = await _customerRepo.AddWithPhonesAsync(customerEntity);
                return Result<int>.Sucess(newCustomerId);
            }
            catch (SqlException ex) when (ex.Number == 2627) // Unique constraint violation
            {
                return Result<int>.Failure("This email is already registered.");
            }
            catch (Exception ex)
            {
                return Result<int>.Failure($"Unexpected error: {ex.Message}");
            }
        }

        // Update Customer
        public async Task<Result> UpdateAsync(UpdateCustomerDto updateCustomerDto)
        {
            var customerForValidation = new CustomerDto
            {
                CustomerName = updateCustomerDto.CustomerName,
                Email = updateCustomerDto.Email,
                Address = updateCustomerDto.Address
            };

            // Validate using the DTO overload that exists
            var validation = Validations.ValidateCustomer(customerForValidation);
            if (!validation.IsSuccess)
                return validation;

            var entity = CustomerMapper.ToEntity(updateCustomerDto);
            await _customerRepo.UpdateAsync(entity);
            return Result.Success();
        }

        // Delete Customer by Id 
        public async Task<Result> DeleteAsync(int customerId)
        {
            try
            {
                await _customerRepo.DeleteAsync(customerId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed To Delete Customer: {ex.Message}");
            }
        }

        // Delete All Customers 
        public async Task<Result> DeleteAllAsync()
        {
            try
            {
                await _customerRepo.DeleteAllAsync();
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Failure($"Failed To Delete All Customers: {ex.Message}");
            }
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
