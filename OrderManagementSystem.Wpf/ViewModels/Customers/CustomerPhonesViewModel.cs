using OrderManagementSystem.Application.Interfaces;
using OrderManagementSystem.Dtos.Customers;
using OrderManagementSystem.Wpf.ClientService.Dialog;
using OrderManagementSystem.Wpf.ClientService.Navigation;
using OrderManagementSystem.Wpf.ClientServices.EvenService;
using OrderManagementSystem.Wpf.Commands;
using OrderManagementSystem.Wpf.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using static OrderManagementSystem.Wpf.Helper.Event.CustomerEvents;

namespace OrderManagementSystem.Wpf.ViewModels.Customers
{
    public class CustomerPhonesViewModel : BaseViewModel
    {
        // --------------- Validations -----------------
        private static readonly Regex PhoneRegex = new(
            @"^\+?[\d\s\-\(\)]{7,15}$",
            RegexOptions.Compiled);

        // --------------- Dependencies -------------------------
        private readonly ICustomerService _customerService;
        private readonly INavigationService _navigationService;
        private readonly IEventBus _eventBus;
        private readonly IDialogService _dialogService;

        // ------------ Backing Fields --------------------------
        private int _customerId;
        private string _customerName = string.Empty;
        private bool _isLoading;
        private string? _errorMessage;

        // Form Fields 
        private CustomerPhoneDto? _selectedPhone;
        private string? _phoneNumber;
        private string _phoneType = "Mobile";
        private bool _isPrimary;

        // -------------- Bound Properties ----------------------
        public int CustomerId
        {
            get => _customerId;
            set
            {
                _customerId = value;
                OnPropertyChanged(nameof(CustomerId));
            }
        }

        public string CustomerName
        {
            get => _customerName;
            set
            {
                _customerName = value;
                OnPropertyChanged(nameof(CustomerName));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public string? ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged(nameof(ErrorMessage));
            }
        }

        // -------------- Form State -------------------------------
        public bool IsEditMode => _selectedPhone != null;

        public CustomerPhoneDto? SelectedPhone
        {
            get => _selectedPhone;
            set
            {
                _selectedPhone = value;

                if (value != null)
                {
                    // Populate Form From The Selected Row
                    PhoneNumber = value.PhoneNumber;
                    PhoneType = value.PhoneType ?? "Mobile";
                    IsPrimary = value.IsPrimary;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEditMode));
                OnPropertyChanged(nameof(FormTitle));
            }
        }

        public string? PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                _phoneNumber = value;
                OnPropertyChanged();
                ErrorMessage = null;
            }
        }

        public string PhoneType
        {
            get => _phoneType;
            set
            {
                _phoneType = value;
                OnPropertyChanged();
            }
        }

        public bool IsPrimary
        {
            get => _isPrimary;
            set
            {
                _isPrimary = value;
                OnPropertyChanged();
            }
        }

        public string FormTitle => IsEditMode ? "Edit Phone Record" : "Add New Phone";

        // ------------------- Collections ---------------------------------------------
        public ObservableCollection<CustomerPhoneDto> CustomerPhones { get; } = new(); // ReadOnly

        // ------------------ Commands -------------------------------------------------
        public ICommand? BackCommand { get; }
        public ICommand? RemovePhoneCommand { get; }
        public ICommand? SavePhoneCommand { get; }
        public ICommand? ClearFormCommand { get; }
        public ICommand? SaveChangesCommand { get; }


        // ------------------ Constructor -----------------------------------------------
        public CustomerPhonesViewModel(
            ICustomerService customerService,
            INavigationService navigationService,
            IEventBus eventBus,
            IDialogService dialogService)
        {
            // Injections
            _customerService = customerService;
            _navigationService = navigationService;
            _eventBus = eventBus;
            _dialogService = dialogService;


            // Init Commands 
            BackCommand = new RelayCommand(
                _ => GoBack(),
                _ => !IsLoading);

            RemovePhoneCommand = new AsyncRelayCommand(
                async obj =>
            {
                if (obj is CustomerPhoneDto phone)
                    await DeletePhoneAsync(phone);
            },
                _ => !IsLoading);

            SavePhoneCommand = new RelayCommand(
                _ => ApplyPhoneToList(),
                _ => !IsLoading);

            ClearFormCommand = new RelayCommand(
                _ => ClearForm(),
                _ => !IsLoading);

            SaveChangesCommand = new AsyncRelayCommand(
                async _ => await SaveAllChangesAsync(),
                _ => !IsLoading);

        }

        // -------------------- Initialization --------------------------------------
        public async Task InitializeAsync(CustomerDto customer)
        {
            if (customer == null)
                return;

            CustomerId = customer.CustomerId;
            CustomerName = customer.CustomerName!;

            await LoadCustomerPhonesAsync();
            ClearForm();
        }

        private async Task LoadCustomerPhonesAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                CustomerPhones.Clear();

                var result = await _customerService.GetPhonesByCustomerIdAsync(CustomerId);

                if (result.IsSuccess && result.Data != null)
                    foreach (var phone in result.Data)
                        CustomerPhones.Add(phone);
                else
                    ErrorMessage = result.Error;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed To Load Phones Records: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        
        // ------------- Delete ------------------------------------------------
        private async Task DeletePhoneAsync(CustomerPhoneDto phone)
        {
            if (phone.PhoneId == 0)
            {
                CustomerPhones.Remove(phone);
                if (ReferenceEquals(SelectedPhone, phone))
                    ClearForm();
                return;
            }

            bool confirmed = await _dialogService.ShowConfirmationAsync(
                $"Are you sure you want to delete the number ({phone.PhoneNumber})?",
                "Confirm Delete");

            if (!confirmed) 
                return;

            try
            {
                IsLoading = true;
                var result = await _customerService.DeletePhoneAsync(phone.PhoneId);

                if (result.IsSuccess)
                {
                    CustomerPhones.Remove(phone);
                    if (ReferenceEquals(SelectedPhone, phone)) 
                        ClearForm();
                    _eventBus.Publish(new CustomerUpdatedEvent(CustomerId));
                }
                else
                {
                    ErrorMessage = result.Error;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Delete Failed : {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // -------------------- Apply Phone To Local List --------------------
        private void ApplyPhoneToList()
        {
            ErrorMessage = null;

            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                ErrorMessage = "Please enter a valid phone number.";
                return;
            }

            if (!PhoneRegex.IsMatch(PhoneNumber.Trim()))
            {
                ErrorMessage = $"\"{PhoneNumber}\" is not a valid phone number.";
                return;
            }

            // When marking this entry as Primary, demote all others first
            if (IsPrimary)
                foreach (var p in CustomerPhones) 
                    p.IsPrimary = false;

            if (IsEditMode)
            {
                // UPDATE existing row
                SelectedPhone!.PhoneNumber = PhoneNumber.Trim();
                SelectedPhone.PhoneType = PhoneType;
                SelectedPhone.IsPrimary = IsPrimary;

                // Replace the item in the collection so the DataGrid refreshes immediately
                int index = CustomerPhones.IndexOf(SelectedPhone);
                if (index >= 0) 
                    CustomerPhones[index] = SelectedPhone;
            }
            else
            {
                // ADD new local row (PhoneId = 0 flags it as unsaved)
                CustomerPhones.Add(new CustomerPhoneDto
                {
                    PhoneId = 0,
                    CustomerId = CustomerId,
                    PhoneNumber = PhoneNumber.Trim(),
                    PhoneType = PhoneType,
                    // Auto-primary when it is the only entry, or explicitly chosen
                    IsPrimary = IsPrimary || !CustomerPhones.Any()
                });
            }

            ClearForm();
        }

        // ---------------------- Persist All Changes --------------------------
        private async Task SaveAllChangesAsync()
        {
            // Validate before touching the server
            if (CustomerPhones.Any(p => string.IsNullOrWhiteSpace(p.PhoneNumber)))
            {
                ErrorMessage = "All phone number fields must be filled in before saving.";
                return;
            }

            if (CustomerPhones.Any() && !CustomerPhones.Any(p => p.IsPrimary))
            {
                ErrorMessage = "Please mark at least one phone number as primary before saving.";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = null;

                var errors = new List<string>();

                foreach (var phone in CustomerPhones)
                {
                    var result = phone.PhoneId == 0
                        ? await _customerService.AddPhoneAsync(phone)
                        : await _customerService.UpdatePhoneAsync(phone);

                    if (!result.IsSuccess && result.Error != null)
                        errors.Add(result.Error);
                }

                if (errors.Any())
                {
                    // Surface all failures rather than stopping at the first one
                    ErrorMessage = string.Join(Environment.NewLine, errors);
                    return;
                }

                _eventBus.Publish(new CustomerUpdatedEvent(CustomerId));
                await _dialogService.ShowInfoAsync("All phone records have been saved successfully.", "Saved");
                GoBack();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Save failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // -------------------------- Helpers ---------------------------------
        private void ClearForm()
        {
            PhoneNumber = string.Empty;
            PhoneType = "Mobile";
            IsPrimary = false;

            // Set the backing field directly to skip the form-population
            // logic in the setter, then notify normally.
            _selectedPhone = null;
            OnPropertyChanged(nameof(SelectedPhone));
            OnPropertyChanged(nameof(IsEditMode));
            OnPropertyChanged(nameof(FormTitle));
        }

        private void GoBack() => _navigationService.NavigateTo<CustomersViewModel>();
        
    }
}
