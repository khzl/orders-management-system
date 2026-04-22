using OrderManagementSystem.Application.Interfaces;
using OrderManagementSystem.Domain.Entities;
using OrderManagementSystem.Dtos.Customers;
using OrderManagementSystem.Wpf.ClientService.Navigation;
using OrderManagementSystem.Wpf.ClientServices.Navigation;
using OrderManagementSystem.Wpf.Commands;
using OrderManagementSystem.Wpf.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;

namespace OrderManagementSystem.Wpf.ViewModels.Customers
{
    public class UpdateCustomerViewModel : BaseViewModel , INotifyPropertyChanged
    {
        // Request Data Customer 
        
        // private field 
        private readonly ICustomerService _customerService;
        private readonly INavigationService _navigationService;

        private int _customerId;
        public int CustomerId
        {
            get => _customerId;
            set
            {
                _customerId = value;
                OnPropertyChanged(nameof(CustomerId));
            }
        }
        
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

        // Property To Show Phones in Screen (list will be Show Items Control) 
        public ObservableCollection<CustomerPhoneDto> CustomerPhones { get; set; } = new();

        // Commands 
        public ICommand? SaveCommand { get; } // ReadOnly 
        public ICommand? CancelCommand { get; } // ReadOnly

        // Command To Operation Phones
        public ICommand? AddPhoneCommand { get; } // ReadOnly
        public ICommand? DeletePhoneCommand { get; } // ReadOnly


        // public Constructor Injection 
        public UpdateCustomerViewModel(ICustomerService customerService, INavigationService navigationService)
        {
            _customerService = customerService;
            _navigationService = navigationService;

            SaveCommand = new AsyncRelayCommand(_ => Save());

            CancelCommand = new RelayCommand(_ => GoBack());

            // method add phone empty for list to make user set 
            AddPhoneCommand = new RelayCommand(_ =>
            {
                CustomerPhones.Add(new CustomerPhoneDto
                {
                    CustomerId = CustomerId,
                    PhoneType = "Mobile"
                });
            });

            DeletePhoneCommand = new RelayCommand(param =>
            {
                if (param is CustomerPhoneDto phone && CustomerPhones.Count > 1)
                    CustomerPhones.Remove(phone);
            });

            // Load Data 
        }

        // method call NavigationService to passing Data 
        public void Load(CustomerDto customerDto)
        {
            if (customerDto == null)
                return;

            CustomerId = customerDto.CustomerId;
            CustomerName = customerDto.CustomerName;
            Email = customerDto.Email;
            Address = customerDto.Address;

            CustomerPhones.Clear();
            foreach (var phone in customerDto.Phones)
            {
                CustomerPhones.Add(phone);
            }
        }

        private async Task Save()
        {
            ErrorMessage = null;

            var updateCustomerDto = new UpdateCustomerDto
            {
                CustomerId = CustomerId,
                CustomerName = CustomerName,
                Email = Email,
                Address = Address,
                CustomerPhones = CustomerPhones.ToList()
            };

            var result = await _customerService.UpdateAsync(updateCustomerDto);

            if (result.IsSuccess)
                _navigationService.NavigateTo<CustomersViewModel>();
            else
                ErrorMessage = result.Error;
        }

        // Cancel -> GoBack
        private void GoBack()
        {
            _navigationService.NavigateTo<CustomersViewModel>();
        }
    }
}
