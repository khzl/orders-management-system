using OrderManagementSystem.Domain.Entities;
using OrderManagementSystem.Dtos.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Application.Mapper
{
    public static class CustomerPhoneMapper
    {
        public static Entity_CustomerPhones ToEntity(CustomerPhoneDto customerPhoneDto)
        {
            return new Entity_CustomerPhones
            {
                PhoneId = customerPhoneDto.PhoneId,
                CustomerId = customerPhoneDto.CustomerId,
                PhoneNumber = customerPhoneDto.PhoneNumber,
                PhoneType = customerPhoneDto.PhoneType,
                IsPrimary = customerPhoneDto.IsPrimary
            };
        }

        public static CustomerPhoneDto ToDto(Entity_CustomerPhones customerPhones)
        {
            return new CustomerPhoneDto
            {
                PhoneId = customerPhones.PhoneId,
                CustomerId = customerPhones.CustomerId,
                PhoneNumber = customerPhones.PhoneNumber,
                PhoneType = customerPhones.PhoneType,
                IsPrimary = customerPhones.IsPrimary
            };
        }

    }
}
