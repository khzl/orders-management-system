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
using OrderManagementSystem.Dtos;

namespace OrderManagementSystem.Wpf.ViewModels.Customers
{
    public class UpdateCustomerViewModel : BaseViewModel
    {
        // Request Data Customer 
        
        // private field 
        private readonly ICustomerService _customerService;
        private readonly INavigationService _navigationService;
        private readonly IEventBus _eventBus;
        private readonly List<int> _deletedPhoneIds = new(); // To Track Deleted Phones

        // Property Binding 
        public int CustomerId { get; set; } // Property Standard
        
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
        public ICommand? RemovePhoneCommand { get; } // ReadOnly
        public ICommand? UpdatePhoneCommand { get; } // ReadOnly
        public ICommand? SetPrimaryCommand { get; } // ReadOnly

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

            AddPhoneCommand = new RelayCommand(_ => AddPhone());

            RemovePhoneCommand = new RelayCommand(obj =>
            {
                if (obj is CustomerPhoneDto phone)
                {
                    if (phone.PhoneId > 0)
                        _deletedPhoneIds.Add(phone.PhoneId);

                    CustomerPhones.Remove(phone);

                    if (CustomerPhones.Any() && !CustomerPhones.Any(p => p.IsPrimary))
                        CustomerPhones.First().IsPrimary = true;
                }
            });

            UpdatePhoneCommand = new AsyncRelayCommand(async obj =>
            {
                if (obj is CustomerPhoneDto phone)
                {
                    var result = await _customerService.UpdatePhoneAsync(phone);

                    if (!result.IsSuccess) 
                        ErrorMessage = result.Error;
                }
            });

            SetPrimaryCommand = new RelayCommand(obj =>
            {
                if (obj is CustomerPhoneDto selected)
                {
                    foreach (var phone in CustomerPhones) 
                        phone.IsPrimary = false;
                    selected.IsPrimary = true;
                }
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

                var primary = CustomerPhones.FirstOrDefault(p => p.IsPrimary);

                foreach (var phone in CustomerPhones)
                    phone.IsPrimary = phone == primary;

                var updateCustomerDto = new UpdateCustomerDto
                {
                    CustomerId = CustomerId,
                    CustomerName = CustomerName,
                    Email = Email,
                    Address = Address,

                    // Current Available On Screen
                    CustomerPhones = CustomerPhones.ToList(),

                    // Deleted Phones Ids To Delete It When Save 
                    DeletedPhoneIds = _deletedPhoneIds
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

        // Add Phone (Local)
        private Task AddPhone()
        {
            CustomerPhones.Add(new CustomerPhoneDto
            {
                CustomerId = CustomerId,
                PhoneNumber = "",
                PhoneType = "Mobile",
                IsPrimary = false
            });

            return Task.CompletedTask;
        }

        // Cancel -> GoBack
        private void GoBack()
        {
            _navigationService.NavigateTo<CustomersViewModel>();
        }

    }
}
