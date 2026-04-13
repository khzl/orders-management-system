using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Wpf.Helper
{
    public class md_RecentOrderRow
    {
        // Property
        public string? OrderId { get; set; }
        public string? CustomerName { get; set; }
        public string? OrderDate { get; set; }
        public string? TotalAmount { get; set; }
        public string? Status { get; set; }
    }
}
