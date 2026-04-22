using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Dtos.Customers
{
    public class CustomerDto
    {
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }

        public List<CustomerPhoneDto> Phones { get; set; } = new();
    }
}
