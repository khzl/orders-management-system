using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Dtos.Customers
{
    public class UpdateCustomerDto
    {
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }

        public List<CustomerPhoneDto> CustomerPhones { get; set; } = new();
        public List<int>? DeletedPhoneIds { get; set; } = new(); // List To Store Deleted Phones Ids To Delete It When Save
    }
}
