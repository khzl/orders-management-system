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
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace OrderManagementSystem.Wpf.ViewModels.Customers
{
    public class AddCustomerViewModel : BaseViewModel
    {

        // ---------------------- Validation Patterns ------------------------
        private static readonly Regex EmailRegex = new(
           @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
           RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly Regex PhoneRegex = new(
            @"^\+?[\d\s\-\(\)]{7,15}$",
            RegexOptions.Compiled);

        // ---------------------- Dependencies ------------------------------ 
        private readonly ICustomerService _customerService;
        private readonly INavigationService _navigationService;
        private readonly IEventBus _eventBus; // add Event After Save 


        // ---------------------- Collections --------------------------------
        public ObservableCollection<CustomerPhoneDto> CustomerPhones { get; } = new(); // ReadOnly


        // ---------------------- Bound Properties ----------------------------
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

        //-------------------- Commands ------------------------
        public ICommand? SaveCommand { get; }
        public ICommand? CancelCommand { get; }
        public ICommand? AddPhoneCommand { get; }
        public ICommand? RemovePhoneCommand { get; }
        public ICommand? SetPrimaryCommand { get; }
        public ICommand? UpdatePhoneCommand { get; }

        // ------------------- Constructor --------------------------
        public AddCustomerViewModel(
            ICustomerService customerService,
            INavigationService navigationService,
            IEventBus eventBus)
        {
            _customerService = customerService;
            _navigationService = navigationService;
            _eventBus = eventBus;

            // Seed One Default Phone row so the form is never empty on open 
            CustomerPhones.Add(new CustomerPhoneDto
            {
                PhoneType = "Mobile",
                IsPrimary = true
            });

            AddPhoneCommand = new RelayCommand(_ =>

                CustomerPhones.Add(new CustomerPhoneDto
                {
                    PhoneType = "Mobile",
                    IsPrimary = false
                }),
                _ => !IsLoading);

            RemovePhoneCommand = new RelayCommand(
                obj =>
                {
                    if (obj is not CustomerPhoneDto phone || CustomerPhones.Count <= 1)
                        return;

                    bool wasPrimary = phone.IsPrimary;
                    CustomerPhones.Remove(phone);

                    // Promote first remaining entry when the primary was removed
                    if (wasPrimary && CustomerPhones.Any())
                        CustomerPhones[0].IsPrimary = true;
                },
                obj => obj is CustomerPhoneDto && CustomerPhones.Count > 1 && !IsLoading);


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

            UpdatePhoneCommand = new RelayCommand(
                _ => ClearError(),
                _ => !IsLoading);

            SaveCommand = new AsyncRelayCommand(_ => SaveAsync(), _ => !IsLoading);
            CancelCommand = new RelayCommand(_ => GoBack(), _ => !IsLoading);
        }

        // --------------------- Save -----------------------------------
        private async Task SaveAsync()
        {
            ErrorMessage = null;
            IsLoading = true;

            try
            {
                if (!ValidateInputs())
                    return;

                EnsureSinglePrimary();

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
            catch (Exception ex)
            {
                ErrorMessage = $"An Unexpected Error Occurred: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // -------------------- Validations -----------------------
        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(CustomerName) || CustomerName.Trim().Length < 3)
            {
                ErrorMessage = "Customer name must be at least 3 characters.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(Email) && !EmailRegex.IsMatch(Email.Trim()))
            {
                ErrorMessage = "Please enter a valid email address.";
                return false;
            }

            if (!CustomerPhones.Any())
            {
                ErrorMessage = "At least one phone number is required.";
                return false;
            }

            foreach (var phone in CustomerPhones)
            {
                if (string.IsNullOrWhiteSpace(phone.PhoneNumber))
                {
                    ErrorMessage = "All phone number fields must be filled in.";
                    return false;
                }

                if (!PhoneRegex.IsMatch(phone.PhoneNumber.Trim()))
                {
                    ErrorMessage = $"\"{phone.PhoneNumber}\" is not a valid phone number.";
                    return false;
                }
            }

            if (!CustomerPhones.Any(p => p.IsPrimary))
            {
                ErrorMessage = "Please mark one phone number as primary.";
                return false;
            }

            return true;
        }


        // -------------------- Helpers ---------------------------
        private void EnsureSinglePrimary()
        {
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
