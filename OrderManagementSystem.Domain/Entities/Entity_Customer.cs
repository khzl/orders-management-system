using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Domain.Entities
{
    public class Entity_Customer
    {
        #region Properties
        public int CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Navigation Property
        public ICollection<Entity_CustomerPhones> CustomerPhones { get; set; } = new List<Entity_CustomerPhones>();

        // فقط للتبع اثناء التحديث 
        public List<int>? DeletedPhoneIds { get; set; } = new();
        #endregion
    }
}
