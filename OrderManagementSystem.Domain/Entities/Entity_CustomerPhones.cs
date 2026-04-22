using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Domain.Entities
{
    public class Entity_CustomerPhones
    {
        #region Properties
        public int PhoneId { get; set; }
        public int CustomerId { get; set; }
        public string? PhoneNumber { get; set; } 
        public string? PhoneType { get; set; } // Mobile , Work , Home
        public bool IsPrimary { get; set; }
        #endregion
    }
}
