using OrderManagementSystem.Application.Interfaces;
using OrderManagementSystem.Domain.Entities;
using OrderManagementSystem.Wpf.ClientService.Navigation;
using OrderManagementSystem.Wpf.Commands;
using OrderManagementSystem.Wpf.Helper;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.ComponentModel;
using System.Collections.ObjectModel;
using OrderManagementSystem.Application.Commons;
using OrderManagementSystem.Dtos.Customers;
using static OrderManagementSystem.Wpf.Helper.Event.CustomerEvents;
using OrderManagementSystem.Wpf.ClientServices.EvenService;

namespace OrderManagementSystem.Wpf.ViewModels.Customers
{
    public class AddCustomerViewModel : BaseViewModel
    {
        // private field 
        private readonly ICustomerService _customerService;
        private readonly INavigationService _navigationService;
        private readonly IEventBus _eventBus; // add Event After Save 


        // List Phones
        public ObservableCollection<CustomerPhoneDto> CustomerPhones { get; set; } = new();


        // Property Binding
        private string? _customerName;
        public string? CustomerName 
        {
            get => _customerName;
            set
            {
                _customerName = value;
                OnPropertyChanged(nameof(CustomerName));
            }
        }

        private string? _email;
        public string? Email 
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        private string? _address;
        public string? Address 
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged(nameof(Address));
            }
        }

        private string? _phoneType;
        public string? PhoneType
        {
            get => _phoneType;
            set
            {
                _phoneType = value;
                OnPropertyChanged(nameof(PhoneType));
            }
        }

        private bool _isPrimary;
        public bool IsPrimary
        {
            get => _isPrimary;
            set
            {
                _isPrimary = value;
                OnPropertyChanged(nameof(IsPrimary));
            }
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        //Commands
        public ICommand? SaveCommand { get; }
        public ICommand? CancelCommand { get; }
        public ICommand? AddPhoneCommand { get; }
        public ICommand? RemovePhoneCommand { get; }

        // Constructor 
        public AddCustomerViewModel(
            ICustomerService customerService,
            INavigationService navigationService,
            IEventBus eventBus)
        {
            _customerService = customerService;
            _navigationService = navigationService;
            _eventBus = eventBus;

            // Add First field Phones Auto when Open Screen
            // رقم واحد افتراضي عند الفتح
            CustomerPhones.Add(new CustomerPhoneDto
            {
                PhoneType = "Mobile",
                IsPrimary = true
            });

            AddPhoneCommand = new RelayCommand(_ => 
            {
                CustomerPhones.Add(new CustomerPhoneDto
                {
                    PhoneType = "Mobile",
                    IsPrimary = false
                });
            });

            RemovePhoneCommand = new RelayCommand(phoneObj =>
            {
                if (phoneObj is CustomerPhoneDto phone && CustomerPhones.Count > 1)
                {
                    CustomerPhones.Remove(phone);

                    // Ensure One Primary Always Exists
                    if (!CustomerPhones.Any(p => p.IsPrimary))
                        CustomerPhones.First().IsPrimary = true;
                }
            });

            SaveCommand = new AsyncRelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => GoBack());
        }

        // Method For Save New Customer  
        private async Task Save()
        {
            ErrorMessage = null;
            IsLoading = true;

            try
            {
                if (!ValidateData())
                    return;

                // Ensure Only One Primary 
                var primary = CustomerPhones.FirstOrDefault(p => p.IsPrimary);
                foreach (var phone in CustomerPhones)
                    phone.IsPrimary = phone == primary;

                var createCustomerDto = new CreateCustomerDto
                {
                    CustomerName = CustomerName,
                    Email = Email,
                    Address = Address,
                    CustomerPhones = CustomerPhones.ToList() // تحويل الـ Collection لقائمة
                };

                var result = await _customerService.CreateAsync(createCustomerDto);

                if (result.IsSuccess)
                {
                    _eventBus.Publish(new CustomerCreatedEvent(result.Data));
                    _navigationService.NavigateTo<CustomersViewModel>();
                }
                else
                {
                    ErrorMessage = result.Error;
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Method to Check Validation Logic
        private bool ValidateData()
        {
            if (string.IsNullOrWhiteSpace(CustomerName) || CustomerName.Length < 3)
            {
                ErrorMessage = "Customer Name Must Be At Least 3 Characters";
                return false;
            }

            if (!CustomerPhones.Any())
            {
                ErrorMessage = "At least one primary phone number is required.";
                return false;
            }

            // فحص كل الأرقام المضافة في القائمة
            foreach (var p in CustomerPhones)
            {
                if (string.IsNullOrWhiteSpace(p.PhoneNumber))
                {
                    ErrorMessage = "Phone Number Is Required..";
                    return false;
                }
            }

            return true;
        }

        // Cancel -> GoBack To CustomersView
        private void GoBack()
        {
            _navigationService.NavigateTo<CustomersViewModel>();
        }

    }
}
