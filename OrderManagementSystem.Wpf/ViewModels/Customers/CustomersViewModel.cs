using OrderManagementSystem.Application.Interfaces;
using OrderManagementSystem.Domain.Entities;
using OrderManagementSystem.Wpf.ClientService.Dialog;
using OrderManagementSystem.Wpf.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace OrderManagementSystem.Wpf.ViewModels.Customers
{
    public class CustomersViewModel : BaseViewModel
    {
        // private field
        private readonly ICustomerService _customerService;
        private readonly IDialogService _dialogService;

        // property List Collection For Customer 
        public ObservableCollection<Entity_Customer>
    }
}
