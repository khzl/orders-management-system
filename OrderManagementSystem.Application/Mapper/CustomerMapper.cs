using OrderManagementSystem.Domain.Entities;
using OrderManagementSystem.Dtos.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Application.Mapper
{
    public static class CustomerMapper
    {
        // Entity -> Dto
        public static CustomerDto ToDto(Entity_Customer entity) => new()
        {
            CustomerId = entity.CustomerId,
            CustomerName = entity.CustomerName,
            Email = entity.Email,
            Address = entity.Address,
            Phones = entity.CustomerPhones?.Select(p => new CustomerPhoneDto
            {
                PhoneId = p.PhoneId,
                CustomerId = p.CustomerId,
                PhoneNumber = p.PhoneNumber,
                PhoneType = p.PhoneType,
                IsPrimary = p.IsPrimary
            }).ToList() ?? new()
        };

        // CreateDto -> Entity
        public static Entity_Customer ToEntity(CreateCustomerDto dto) => new()
        {
            CustomerName = dto.CustomerName,
            Email = dto.Email,
            Address = dto.Address,
            CustomerPhones = dto.CustomerPhones?.Select(p => new Entity_CustomerPhones
            {
                PhoneId = p.PhoneId,
                PhoneNumber = p.PhoneNumber,
                PhoneType = p.PhoneType,
                IsPrimary = p.IsPrimary
            }).ToList() ?? new()
        };

        // UpdateDto -> Entity
        public static Entity_Customer ToEntity(UpdateCustomerDto dto) => new()
        {
            CustomerId = dto.CustomerId,
            CustomerName = dto.CustomerName,
            Email = dto.Email,
            Address = dto.Address,

            DeletedPhoneIds = dto.DeletedPhoneIds,

            CustomerPhones = dto.CustomerPhones
            .Select(p => new Entity_CustomerPhones
            {
                PhoneId = p.PhoneId,
                CustomerId = p.CustomerId,
                PhoneNumber = p.PhoneNumber,
                PhoneType = p.PhoneType,
                IsPrimary = p.IsPrimary
            }).ToList()
        };

    }
}
