using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Dtos.Customers
{
    public class CustomerPhoneDto
    {
        public int PhoneId { get; set; }
        public int CustomerId { get; set; }
        public string? PhoneNumber { get; set; }
        public string? PhoneType { get; set; }
        public bool IsPrimary { get; set; }
    }
}
