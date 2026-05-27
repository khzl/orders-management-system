using Microsoft.Data.SqlClient;
using OrderManagementSystem.Application.Commons;
using OrderManagementSystem.Application.Interfaces;
using OrderManagementSystem.Application.Mapper;
using OrderManagementSystem.Domain.Entities;
using OrderManagementSystem.Dtos.Customers;
using OrderManagementSystem.Infrastructure.Interfaces;
using OrderManagementSystem.Shared;
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
        public async Task<Result<PaginationResult<CustomerDto>>> GetAllAsync(int pageNumber,int pageSize)
        {
            var result = 
                await _customerRepo.GetAllAsync(pageNumber,pageSize);

            var dtos = 
                result.Data.Select(CustomerMapper.ToDto).ToList();

            return Result<PaginationResult<CustomerDto>>.Success(
                new PaginationResult<CustomerDto>
                {
                    Data = dtos,
                    CurrentPage = result.CurrentPage,
                    PageSize = result.PageSize,
                    TotalCount = result.TotalCount
                });
        }


        // Get Customer By Id
        public async Task<Result<CustomerDto>> GetByIdAsync(int customerId)
        {
            if (customerId <= 0)
                return Result<CustomerDto>.Failure("Invalid customer Id");

            var customerEntity = await _customerRepo.GetByIdAsync(customerId);

            if (customerEntity is null)
                return Result<CustomerDto>.Failure($"Customer {customerId} Not Found");

            return Result<CustomerDto>.Success(CustomerMapper.ToDto(customerEntity));
        }

        //  Create Customer With Phones 
        public async Task<Result<int>> CreateAsync(CreateCustomerDto createCustomerDto)
        {
            if (createCustomerDto == null)
                return Result<int>.Failure("Invalid Request");

            // validation Name 
            if (string.IsNullOrWhiteSpace(createCustomerDto.CustomerName) ||
                createCustomerDto.CustomerName.Trim().Length < 3)
                return Result<int>.Failure("Customer Name Must Be At Least 3 Characters");

            // validation Phones 
            if (createCustomerDto.CustomerPhones == null ||
                createCustomerDto.CustomerPhones.Count == 0)
                return Result<int>.Failure("At Least One Phone Number Is Required");

            foreach (var phone in createCustomerDto.CustomerPhones)
            {
                if (string.IsNullOrWhiteSpace(phone.PhoneNumber))
                    return Result<int>.Failure("Phone Number Is Required");

                var phoneValidation = Validations.ValidatePhone(phone.PhoneNumber);
                if (!phoneValidation.IsSuccess)
                    return Result<int>.Failure(phoneValidation.Error!);
            }

            try
            {
                var entity = CustomerMapper.ToEntity(createCustomerDto);
                var newCustomerId = await _customerRepo.AddWithPhonesAsync(entity);
                return Result<int>.Success(newCustomerId);
            }
            catch (SqlException ex) when (ex.Number == 2627)
            {
                return Result<int>.Failure("Email Already Exists");
            }
            catch (Exception ex)
            {
                return Result<int>.Failure(ex.Message);
            }
        }


        // Update Customer
        public async Task<Result> UpdateAsync(UpdateCustomerDto updateCustomerDto)
        {
            if (updateCustomerDto.CustomerId <= 0)
                return Result.Failure("Invalid Customer Id");

            if (string.IsNullOrWhiteSpace(updateCustomerDto.CustomerName) ||
                updateCustomerDto.CustomerName.Trim().Length < 3)
                return Result.Failure("Customer Name Must Be At Least 3 Characters");

            try
            {
                var entity = CustomerMapper.ToEntity(updateCustomerDto);
                await _customerRepo.UpdateAsync(entity);
                return Result.Success();
            }
            catch(Exception ex)
            {
                return Result.Failure(ex.Message);
            }
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
        public async Task<Result<CustomerPhoneDto>> AddPhoneAsync(CustomerPhoneDto customerPhoneDto)
        {
            if (customerPhoneDto == null)
                return Result<CustomerPhoneDto>.Failure("Phone Data Is Required..");

            var customer = await _customerRepo.GetByIdAsync(customerPhoneDto.CustomerId);
            if (customer == null)
                return Result<CustomerPhoneDto>.Failure("Customer Not Found");

            if (customerPhoneDto.CustomerId <= 0)
                return Result<CustomerPhoneDto>.Failure("Invalid Customer Id");

            var validation = Validations.ValidatePhone(customerPhoneDto.PhoneNumber!);
            if (!validation.IsSuccess)
            {
                return Result<CustomerPhoneDto>.Failure(validation.Error ?? "Invalid Phone Number");
            }
            await _customerRepo.AddPhoneAsync(
                customerPhoneDto.CustomerId,
                customerPhoneDto.PhoneNumber!,
                customerPhoneDto.PhoneType ?? "Mobile",
                customerPhoneDto.IsPrimary);

            return Result<CustomerPhoneDto>.Success(customerPhoneDto);
        }

        // Delete Phone 
        public async Task<Result> DeletePhoneAsync(int phoneId)
        {
            if (phoneId <= 0)
                return Result.Failure("Invalid Phone Id");

            await _customerRepo.DeletePhoneAsync(phoneId);
            return Result.Success();
        }

        // Update Phone 
        public async Task<Result> UpdatePhoneAsync(CustomerPhoneDto phoneDto)
        {
            if (phoneDto.PhoneId <= 0)
                return Result.Failure("Invalid phone");

            var validation = Validations.ValidatePhone(phoneDto.PhoneNumber!);
            if (!validation.IsSuccess)
                return validation;

            var phone = new CustomerPhoneDto
            {
                PhoneId = phoneDto.PhoneId,
                PhoneNumber = phoneDto.PhoneNumber,
                PhoneType = phoneDto.PhoneType,
                IsPrimary = phoneDto.IsPrimary
            };

            await _customerRepo.UpdatePhoneAsync(CustomerPhoneMapper.ToEntity(phone));

            return Result.Success();
        }

        // GetPhones By CustomerId 
        public async Task<Result<IEnumerable<CustomerPhoneDto>>> GetPhonesByCustomerIdAsync(int customerId)
        {
            if (customerId <= 0)
                return Result<IEnumerable<CustomerPhoneDto>>.Failure("Invalid Customer Id");

            var phones = await _customerRepo.GetPhonesByCustomerIdAsync(customerId);

            var dtos = phones.Select(p => new CustomerPhoneDto
            {
                PhoneId = p.PhoneId,
                CustomerId = p.CustomerId,
                PhoneNumber = p.PhoneNumber,
                PhoneType = p.PhoneType,
                IsPrimary = p.IsPrimary
            }).ToList();

            return Result<IEnumerable<CustomerPhoneDto>>.Success(dtos);
        }


    }
}
