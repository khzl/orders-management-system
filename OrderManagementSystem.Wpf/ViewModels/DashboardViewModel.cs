using OrderManagementSystem.Wpf.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace OrderManagementSystem.Wpf.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        // Property List Collection
        public ObservableCollection<md_RecentOrderRow>? RecentOrders { get; set; }

        // Constructor 
        public DashboardViewModel()
        {
            RecentOrders = new ObservableCollection<md_RecentOrderRow>
            {
                new md_RecentOrderRow
                {
                    OrderId = "#1001",
                    CustomerName = "Khazaal Emad",
                    OrderDate = "2026-04-12",
                    TotalAmount = "240$",
                    Status = "Completed"
                },
                new md_RecentOrderRow
                {
                    OrderId = "#1002",
                    CustomerName = "Omer Ali",
                    OrderDate = "2026-04-11",
                    TotalAmount = "250$",
                    Status = "Pending"
                },
                new md_RecentOrderRow
                {
                    OrderId = "#1003",
                    CustomerName = "Omer Yaser",
                    OrderDate = "2026-04-13",
                    TotalAmount = "350$",
                    Status = "Completed"
                },
            };
        }

    }
}
