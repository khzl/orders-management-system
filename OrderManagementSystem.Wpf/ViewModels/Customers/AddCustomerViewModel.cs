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

namespace OrderManagementSystem.Wpf.ViewModels.Customers
{
    public class AddCustomerViewModel : BaseViewModel
    {
        // private field 
        private readonly ICustomerService _customerService;
        private readonly INavigationService _navigationService;

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

        private string? _phone;
        public string? Phone 
        {
            get => _phone;
            set
            {
                _phone = value;
                OnPropertyChanged(nameof(Phone));
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

        // private field 
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

        // List Phones
        public ObservableCollection<CustomerPhoneDto> CustomerPhones { get; set; } = new();
       
        //Commands
        public ICommand? SaveCommand { get; }
        public ICommand? CancelCommand { get; }
        public ICommand? AddPhoneCommand { get; }
        public ICommand? RemovePhoneCommand { get; }

        // Constructor 
        public AddCustomerViewModel(ICustomerService customerService,INavigationService navigationService)
        {
            _customerService = customerService;
            _navigationService = navigationService;

            // Add First field Phones Auto when Open Screen
            // رقم واحد افتراضي عند الفتح
            CustomerPhones.Add(new CustomerPhoneDto
            {
                PhoneType = "Mobile",
                IsPrimary = true
            });

            AddPhoneCommand = new RelayCommand(_ => CustomerPhones.Add(new CustomerPhoneDto
            {
                PhoneType = "Mobile"
            }));

            RemovePhoneCommand = new RelayCommand(phoneObj =>
            {
                if (phoneObj is CustomerPhoneDto phone && CustomerPhones.Count > 1)
                    CustomerPhones.Remove(phone);
            });

            SaveCommand = new AsyncRelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => GoBack());
        }

        // Save 
        private async Task Save()
        {
            ErrorMessage = null;

            if (!ValidateData())
                return;

            var createCustomerDto = new CreateCustomerDto
            {
                CustomerName = CustomerName,
                Email = Email,
                Address = Address,
                CustomerPhones = CustomerPhones.ToList() // تحويل الـ Collection لقائمة
            };

            Result<int> result;

            if (createCustomerDto.CustomerPhones.Count == 1)
            {
                var firstPhone = createCustomerDto.CustomerPhones.First();
                // validation earlier ensures PhoneNumber is not null/whitespace; use `!` to satisfy the compiler.
                result = await _customerService.CreateAsync(createCustomerDto);
            }
            else
            {
                // if be many phones used method transaction 
                result = await _customerService.CreateWithPhonesAsync(createCustomerDto);
            }

            // result Handling
            if (result.IsSuccess)
            {
                // After Save GoBack Customers View
                _navigationService.NavigateTo<CustomersViewModel>();
            }
            else
            {
                ErrorMessage = result.Error;
            }
        }

        // Method to Check Validation Logic
        private bool ValidateData()
        {
            if (string.IsNullOrWhiteSpace(CustomerName))
            {
                ErrorMessage = "Customer Name is required.";
                return false;
            }

            if (CustomerPhones == null || !CustomerPhones.Any() || string.IsNullOrWhiteSpace(CustomerPhones[0].PhoneNumber))
            {
                ErrorMessage = "At least one primary phone number is required.";
                return false;
            }

            // فحص كل الأرقام المضافة في القائمة
            foreach (var p in CustomerPhones)
            {
                if (string.IsNullOrWhiteSpace(p.PhoneNumber) || p.PhoneNumber.Length < 10)
                {
                    ErrorMessage = "All phone numbers must be valid and at least 10 digits.";
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
