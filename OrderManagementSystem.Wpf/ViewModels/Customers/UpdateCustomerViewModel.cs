using OrderManagementSystem.Application.Interfaces;
using OrderManagementSystem.Domain.Entities;
using OrderManagementSystem.Dtos;
using OrderManagementSystem.Dtos.Customers;
using OrderManagementSystem.Wpf.ClientService.Dialog;
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
using System.Text.RegularExpressions;
using System.Windows.Input;
using static OrderManagementSystem.Wpf.Helper.Event.CustomerEvents;

namespace OrderManagementSystem.Wpf.ViewModels.Customers
{
    public class UpdateCustomerViewModel : BaseViewModel
    {
        // --------- Validations ------------------------------
        private static readonly Regex EmailRegex = new(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex PhoneRegex = new(
            @"^\+?[\d\s\-\(\)]{7,15}$",
            RegexOptions.Compiled);


        // -------------- Dependencies --------------------------------------------- 
        private readonly ICustomerService _customerService;
        private readonly INavigationService _navigationService;
        private readonly IEventBus _eventBus;
        private readonly IDialogService _dialogService;

        // Tracks phoneIds that were removed locally - sent to the server on Save
        private readonly List<int> _deletedPhoneIds = new(); // To Track Deleted Phones

        // ----------------- Bound Properties ---------------------------------------
        public int CustomerId { get; private set; } // Property Standard
        
        private string? _customerName;
        public string? CustomerName 
        { 
            get => _customerName;
            set 
            {
                _customerName = value;
                OnPropertyChanged(nameof(CustomerName));
                ClearError();
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
                ClearError();
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
                ClearError();
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
                CommandManager.InvalidateRequerySuggested();
            }
        }

        // Property To Show Phones in Screen (list will be Show Items Control) 
        public ObservableCollection<CustomerPhoneDto> CustomerPhones { get; } = new();

        // ---------------- Commands ----------------------------------------------------
        public ICommand? SaveCommand { get; } // ReadOnly 
        public ICommand? CancelCommand { get; } // ReadOnly

        // Command To Operation Phones
        public ICommand? AddPhoneCommand { get; } // ReadOnly
        public ICommand? RemovePhoneCommand { get; } // ReadOnly
        public ICommand? UpdatePhoneCommand { get; } // ReadOnly
        public ICommand? SetPrimaryCommand { get; } // ReadOnly

        // ------------------ Constructor ------------------------------------------------
        public UpdateCustomerViewModel(
            ICustomerService customerService,
            INavigationService navigationService,
            IEventBus eventBus,
            IDialogService dialogService)
        {
            _customerService = customerService;
            _navigationService = navigationService;
            _eventBus = eventBus;
            _dialogService = dialogService;


            SaveCommand = new AsyncRelayCommand(
                _ => SaveAsync(),
                _ => !IsLoading);

            CancelCommand = new RelayCommand(
                _ => GoBack(),
                _ => !IsLoading);

            AddPhoneCommand = new RelayCommand(
                _ => AddPhoneLocal(),
                _ => !IsLoading);

            RemovePhoneCommand = new AsyncRelayCommand(
                async obj =>
            {
                if (obj is not CustomerPhoneDto phone)
                    return;

                // Confirm before staging a delete for an already-persisted phone
                if (phone.PhoneId > 0)
                {
                    bool confirmed = await _dialogService.ShowConfirmationAsync(
                        $"Remove \"{phone.PhoneNumber}\"? It Will Be Deleted When You Save.",
                        "Remove Phone");

                    if (!confirmed)
                        return;

                    _deletedPhoneIds.Add(phone.PhoneId);
                }

                CustomerPhones.Remove(phone);
                EnsureSinglePrimary();
            },
                _ => !IsLoading);

            UpdatePhoneCommand = new RelayCommand(
                _ => ClearError(),
                _ => !IsLoading);

            SetPrimaryCommand = new RelayCommand(
                obj =>
            {
                if (obj is not CustomerPhoneDto selected)
                    return;
                foreach (var p in CustomerPhones)
                    p.IsPrimary = false;
                selected.IsPrimary = true;
            },
                _ => !IsLoading);
        }


        // ----------------- Initialization ---------------------------------------------
        public void Load(object parameter)
        {
            if (parameter is not UpdateCustomerDto dto)
                return;

            CustomerId = dto.CustomerId;
            CustomerName = dto.CustomerName;
            Email = dto.Email;
            Address = dto.Address;

            _deletedPhoneIds.Clear();
            CustomerPhones.Clear();

            if (dto.CustomerPhones != null)
            {
                foreach (var phone in dto.CustomerPhones)
                    CustomerPhones.Add(phone);
            }
        }

        // ------------------- Save ---------------------------------------
        private async Task SaveAsync()
        {
            ErrorMessage = null;

            if (!ValidateInputs())
                return;

            IsLoading = true;

            try
            {

                EnsureSinglePrimary();

                var dto = new UpdateCustomerDto
                {
                    CustomerId = CustomerId,
                    CustomerName = CustomerName!.Trim(),
                    Email = NullIfEmpty(Email),
                    Address = NullIfEmpty(Address),
                    CustomerPhones = CustomerPhones.ToList(),
                    DeletedPhoneIds = _deletedPhoneIds
                };

                var result = await _customerService.UpdateAsync(dto);

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
            catch(Exception ex)
            {
                ErrorMessage = $"An Unexpected Error Occurred: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ---------------------- Validations --------------------------------------
        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(CustomerName) || CustomerName.Trim().Length < 3)
            {
                ErrorMessage = "Customer Name Must Be At Least 3 Characters...";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Email) && !EmailRegex.IsMatch(Email.Trim()))
            {
                ErrorMessage = "Please Enter a Valid Email Address..";
                return false;
            }

            if (!CustomerPhones.Any())
            {
                ErrorMessage = "At Least One Phone Number Is Required....";
                return false;
            }

            foreach (var phone in CustomerPhones)
            {
                if (string.IsNullOrWhiteSpace(phone.PhoneNumber))
                {
                    ErrorMessage = "All Phone Number Fields Must Be Filled In...";
                    return false;
                }

                if (!PhoneRegex.IsMatch(phone.PhoneNumber.Trim()))
                {
                    ErrorMessage = $"\"{phone.PhoneNumber}\" is not a valid phone number ...";
                    return false;
                }
            }

            if (!CustomerPhones.Any(p => p.IsPrimary))
            {
                ErrorMessage = "Please Mark One Phone Number As Primary..";
                return false;
            }

            return true;
        }

        // ----------------- Local Phone Operations ----------------------------------------
        private void AddPhoneLocal()
        {
            CustomerPhones.Add(new CustomerPhoneDto
            {
                CustomerId = CustomerId,
                PhoneNumber = string.Empty,
                PhoneType = "Mobile",
                IsPrimary = !CustomerPhones.Any() // auto-primary if first
            });
        }

        // ------------------- Helpers -----------------------------------------------------
        private void EnsureSinglePrimary()
        {
            if (!CustomerPhones.Any())
                return;

            var primary = CustomerPhones.FirstOrDefault(p => p.IsPrimary)
                ?? CustomerPhones.First();

            foreach (var phone in CustomerPhones)
                phone.IsPrimary = phone == primary;
        }

        private void ClearError() => ErrorMessage = null;

        private void GoBack() => _navigationService.NavigateTo<CustomersViewModel>();

        private static string? NullIfEmpty(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    }

}
