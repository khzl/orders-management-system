using OrderManagementSystem.Domain.Entities;
using OrderManagementSystem.Dtos.Customers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace OrderManagementSystem.Application.Commons
{
    public class Validations
    {
        // Validations business Role

        // Validation Customer 
        public static Result ValidateCustomer(CustomerDto customer)
        {
            if (string.IsNullOrWhiteSpace(customer.CustomerName))
                return Result.Failure("Name Is Required");

            if (customer.CustomerName.Length < 3)
                return Result.Failure("Name must be at least 3 characters");

            if (string.IsNullOrWhiteSpace(customer.Email))
                return Result.Failure("Email is Required");

            if (!IsValidEmail(customer.Email))
                return Result.Failure("Invalid Email format");

            return Result.Success();
        }

        // Validation Phones
        public static Result ValidatePhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return Result.Failure("Phone Number Is Required");

            if (phone.Length < 4)
                return Result.Failure("Phone Number Too Short");

            return Result.Success();
        }

        // Validation Email
        public static bool IsValidEmail(string email) =>
            Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
    }
}
