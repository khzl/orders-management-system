using OrderManagementSystem.Application.Interfaces;
using OrderManagementSystem.Domain.Entities;
using OrderManagementSystem.Dtos.Customers;
using OrderManagementSystem.Wpf.ClientService.Navigation;
using OrderManagementSystem.Wpf.ClientServices.EvenService;
using OrderManagementSystem.Wpf.ClientServices.Navigation;
using OrderManagementSystem.Wpf.Commands;
using OrderManagementSystem.Wpf.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows.Input;
using static OrderManagementSystem.Wpf.Helper.Event.CustomerEvents;

namespace OrderManagementSystem.Wpf.ViewModels.Customers
{
    public class UpdateCustomerViewModel : BaseViewModel
    {
        // Request Data Customer 
        
        // private field 
        private readonly ICustomerService _customerService;
        private readonly INavigationService _navigationService;
        private readonly IEventBus _eventBus;

        // Property Binding 
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

        // Property To Show Phones in Screen (list will be Show Items Control) 
        public ObservableCollection<CustomerPhoneDto> CustomerPhones { get; set; } = new();

        // Commands 
        public ICommand? SaveCommand { get; } // ReadOnly 
        public ICommand? CancelCommand { get; } // ReadOnly

        // Command To Operation Phones
        public ICommand? AddPhoneCommand { get; } // ReadOnly
        public ICommand? DeletePhoneCommand { get; } // ReadOnly


        // public Constructor Injection 
        public UpdateCustomerViewModel(
            ICustomerService customerService,
            INavigationService navigationService,
            IEventBus eventBus)
        {
            _customerService = customerService;
            _navigationService = navigationService;
            _eventBus = eventBus;

            SaveCommand = new AsyncRelayCommand(_ => Save());

            CancelCommand = new RelayCommand(_ => GoBack());

            // method add phone empty for list to make user set 
            AddPhoneCommand = new AsyncRelayCommand(_ => AddPhone());

            DeletePhoneCommand = new AsyncRelayCommand(async obj =>
            {
                if (obj is CustomerPhoneDto phone)
                    await DeletePhone(phone);
            });
        }


        // method call NavigationService to passing Data 
        public void Load(object parameter)
        {
            if (parameter is not CustomerDto customerDto)
                return;

            CustomerId = customerDto.CustomerId;
            CustomerName = customerDto.CustomerName;
            Email = customerDto.Email;
            Address = customerDto.Address;
            
            CustomerPhones.Clear();
            if (customerDto.Phones != null)
            {
                foreach (var phone in customerDto.Phones)
                    CustomerPhones.Add(phone);
            }
        }

        // Here Save Customer Only Just 
        private async Task Save()
        {
            ErrorMessage = null;

            if (string.IsNullOrWhiteSpace(CustomerName) || CustomerName.Length < 3)
            {
                ErrorMessage = "Customer Name Must Be At 3 Characters.";
                return;
            }

            try
            {
                IsLoading = true;

                var updateCustomerDto = new UpdateCustomerDto
                {
                    CustomerId = CustomerId,
                    CustomerName = CustomerName,
                    Email = Email,
                    Address = Address
                };

                var result = await _customerService.UpdateAsync(updateCustomerDto);

                if (result.IsSuccess)
                {
                    // ✅ أطلق الـ Event — CustomersViewModel يسمعه ويعمل Reload تلقائياً
                    _eventBus.Publish(new CustomerUpdatedEvent(CustomerId));
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

        // Add Phone (DB)
        private async Task AddPhone()
        {
            var newPhone = new CustomerPhoneDto
            {
                CustomerId = CustomerId,
                PhoneNumber = "",
                PhoneType = "Mobile",
                IsPrimary = false
            };

            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var result = await _customerService.AddPhoneAsync(newPhone);

                if (result.IsSuccess)
                {
                    if (result.Data != null)
                    {
                        CustomerPhones.Add(result.Data);
                    }
                }
                else
                {
                    ErrorMessage = "Failed To Add Phone: " + result.Error;
                }
                
            }
            catch (Exception ex)
            {
                ErrorMessage = "Error Adding Phone: " + ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Delete Phone (DB) 
        private async Task DeletePhone(CustomerPhoneDto customerPhoneDto)
        {
            if (customerPhoneDto == null)
                return;

            try
            {
                IsLoading = true;
                // عملية الحذف تعتمد على  PhoneId و هذا مهم لعملية الحذف 
                if (customerPhoneDto.PhoneId > 0)
                {
                    await _customerService.DeletePhoneAsync(customerPhoneDto.PhoneId);
                }
                CustomerPhones.Remove(customerPhoneDto);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Cancel -> GoBack
        private void GoBack()
        {
            _navigationService.NavigateTo<CustomersViewModel>();
        }
    }
}
