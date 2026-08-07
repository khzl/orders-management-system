using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Wpf.Helper
{
    public class md_RecentOrderRow
    {
        // Property
        public string OrderId { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }

        // Expected Value : "Pending", "Complete", "Cancelled"
        public string Status { get; set; } = string.Empty;
    }
}
