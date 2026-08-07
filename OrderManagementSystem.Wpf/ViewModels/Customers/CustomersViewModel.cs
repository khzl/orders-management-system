using OrderManagementSystem.Application.Interfaces;
using OrderManagementSystem.Dtos.Customers;
using OrderManagementSystem.Wpf.ClientService.Dialog;
using OrderManagementSystem.Wpf.ClientService.Navigation;
using OrderManagementSystem.Wpf.ClientServices.EvenService;
using OrderManagementSystem.Wpf.Commands;
using OrderManagementSystem.Wpf.Helper;
using OrderManagementSystem.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using static OrderManagementSystem.Wpf.Helper.Event.CustomerEvents;

namespace OrderManagementSystem.Wpf.ViewModels.Customers
{
    public class CustomersViewModel : BaseViewModel
    {
        // --------------------------- Dependencies -------------------------------------
        private readonly ICustomerService _customerService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IEventBus _eventBus;

        // Buffer - full page of items returned by the server 
        private List<CustomerDto> _allCustomers = new();

        // EventBus Handler references kept for clean Unsubscribe in Dispose()
        private readonly Action<CustomerCreatedEvent> _onCreated;
        private readonly Action<CustomerUpdatedEvent> _onUpdated;
        private readonly Action<CustomerDeletedEvent> _onDeleted;
        private readonly Action<CustomerDeletedAllEvent> _onDeletedAll;


        // ------------------------------- Collections -----------------------------------
        public RangeObservableCollection<CustomerDto> Customers { get;} = new();

        // ------------------- Bound properties ---------------------------------------------
        private CustomerDto? _selectedCustomer;

        public CustomerDto? SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                _selectedCustomer = value;
                OnPropertyChanged(nameof(SelectedCustomer));
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
        
        private string? _searchText;
        public string? SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
                ResetPageAndLoad();
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading == value)
                    return; 
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
            }
        }

        // ---------------------------- Pagination --------------------------------- 
        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage == value || value < 1)
                    return; 

                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));

                _ = LoadDataAsync();
            }
        }

        private int _totalPages = 1;
        public int TotalPages
        {
            get => _totalPages;
            set
            {
                if (_totalPages != value)
                {
                    _totalPages = Math.Max(1, value);
                    OnPropertyChanged(nameof(TotalPages));
                }
            }
        }

        private int _pageSize = 10;
        public int PageSize
        {
            get => _pageSize;
            set
            {
                _pageSize = value;
                OnPropertyChanged(nameof(PageSize));
            }
        }

        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            set
            {
                _totalCount = value;
                OnPropertyChanged(nameof(TotalCount));
            }
        }

        // ---------------------- Search ------------------------------------------------
        private en_CustomerSearchType? _selectedSearchType = en_CustomerSearchType.All;
        public en_CustomerSearchType? SelectedSearchType
        {
            get => _selectedSearchType;
            set
            {
                _selectedSearchType = value;
                OnPropertyChanged(nameof(SelectedSearchType));
                ResetPageAndLoad();
            }
        }

        public IEnumerable<en_CustomerSearchType> SearchTypes =>
            Enum.GetValues(typeof(en_CustomerSearchType)).Cast<en_CustomerSearchType>();


        // ------------------- Commands ------------------------------------------------
        public ICommand? LoadCommand { get; }
        public ICommand? AddCommand { get; }
        public ICommand? DeleteCommand { get; } 
        public ICommand? DeleteAllCommand { get; } 
        public ICommand? EditCommand { get; }
        public ICommand? NavigateToPhonesCommand { get; } 

        // -------------------- Constructor --------------------------------------------
        public CustomersViewModel(
            ICustomerService customerService, 
            INavigationService navigationService,
            IDialogService dialogService,
            IEventBus eventBus)
        {
            // Injections
            _customerService = customerService;
            _navigationService = navigationService;
            _dialogService = dialogService;
            _eventBus = eventBus;

            // EventBus subscriptions - handlers stored so Dispose() can remove them

            _onCreated = async _ => await LoadDataAsync();
            _onUpdated = async _ => await LoadDataAsync();
            _onDeleted = async _ => await LoadDataAsync();

            _onDeletedAll = _ => App.Current.Dispatcher.InvokeAsync(() =>
            {
                _allCustomers.Clear();
                Customers.Clear();
                TotalCount = 0;
                TotalPages = 1;
                CurrentPage = 1;
            });

            _eventBus.Subscribe(_onCreated);
            _eventBus.Subscribe(_onUpdated);
            _eventBus.Subscribe(_onDeleted);
            _eventBus.Subscribe(_onDeletedAll);

            // Commands - all include CanExecute predicates so UI disables during load

            LoadCommand = new AsyncRelayCommand(
                async _ => await LoadDataAsync(),
                _ => !IsLoading);

            AddCommand = new AsyncRelayCommand(
                async _ =>
                {
                    _navigationService.NavigateTo<AddCustomerViewModel>();
                    await Task.CompletedTask;
                },
                _ => !IsLoading);

            EditCommand = new AsyncRelayCommand(
                async obj =>
                {
                    if (obj is not CustomerDto dto)
                        return;
                    _navigationService.NavigateTo<UpdateCustomerViewModel>(MapToUpdateDto(dto));
                    await Task.CompletedTask;
                },
                obj => obj is CustomerDto && !IsLoading);

            DeleteCommand = new AsyncRelayCommand(
                async obj =>
            {
                if (obj is CustomerDto customerDto)
                    await DeleteCustomerAsync(customerDto);
            },
                obj => obj is CustomerDto && !IsLoading);

            DeleteAllCommand = new AsyncRelayCommand(
                async _ => await DeleteAllCustomersAsync(),
                _ => !IsLoading && Customers.Any());

            NavigateToPhonesCommand = new AsyncRelayCommand(
                async obj =>
                {
                    if (obj is not CustomerDto customerDto)
                        return;
                    _navigationService.NavigateTo<CustomerPhonesViewModel>(customerDto);
                    await Task.CompletedTask;
                },
                obj => obj is CustomerDto && !IsLoading);
        }

        // ---------------- Data Loading ------------------------------------------------
        private async Task LoadDataAsync()
        {
            if (IsLoading)
                return; // Guard against multiple concurrent loads

            ErrorMessage = null;

            try
            {
                IsLoading = true;

                var result =
                    await _customerService.GetAllAsync(
                        CurrentPage,
                        PageSize,
                        SelectedSearchType,
                        SearchText?.Trim());

                if (result.IsSuccess)
                {
                    var pagedData = result.Data;

                    _allCustomers = pagedData?.Data?.ToList() ?? new List<CustomerDto>();

                    Customers.ReplaceRange(_allCustomers);

                    TotalPages = pagedData?.TotalPages ?? 1;
                    TotalCount = pagedData?.TotalCount ?? _allCustomers.Count;
                }
                else
                {
                    ErrorMessage = result.Error;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed To Load Customers: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // لاعداة ضبط الصفحة عند بدء بحث جديد
        private void ResetPageAndLoad()
        {
            if (CurrentPage == 1)
            {
                _ = LoadDataAsync();
            }
            else
            {
                CurrentPage = 1;
            }
        }

        // ----------------------- Delete One & Delete All -------------------------------------------
        private async Task DeleteCustomerAsync(CustomerDto customerDto)
        {
            bool confirmed = await _dialogService.ShowConfirmationAsync(
                $"Are You Sure You Want To Delete \"{customerDto.CustomerName}\"?",
                "Confirm Delete");

            if (!confirmed)
                return;

            try
            {
                IsLoading = true;

                var result = await _customerService.DeleteAsync(customerDto.CustomerId);

                if (result.IsSuccess)
                {
                    // EventBus يتولى الـ Reload — ما نحتاج نستدعي LoadData يدوياً
                    _eventBus.Publish(new CustomerDeletedEvent(customerDto.CustomerId));
                }
                else
                {
                    ErrorMessage = result.Error;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Delete failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteAllCustomersAsync()
        {
            if (!Customers.Any())
                return;

            bool confirmed = await _dialogService.ShowConfirmationAsync(
                "This Will Permanently Delete All Customer Records , This Cannot Be undone..",
                "Delete All - Are You Sure?");

            if (!confirmed)
                return;

            try
            {
                IsLoading = true;
                var result = await _customerService.DeleteAllAsync();

                if (result.IsSuccess)
                {
                    _eventBus.Publish(new CustomerDeletedAllEvent());
                }
                else
                {
                    ErrorMessage = result.Error;
                }
            }
            catch(Exception ex)
            {
                ErrorMessage = $"Delete All Failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ---------------------------- Mapping ---------------------------------------
        private static UpdateCustomerDto MapToUpdateDto(CustomerDto source) =>
            new UpdateCustomerDto
            {
                CustomerId = source.CustomerId,
                CustomerName = source.CustomerName,
                Email = source.Email,
                Address = source.Address,
                CustomerPhones = source.Phones?.Select(p => new CustomerPhoneDto
                {
                    PhoneId = p.PhoneId,
                    CustomerId = p.CustomerId,
                    PhoneNumber = p.PhoneNumber,
                    PhoneType = p.PhoneType,
                    IsPrimary = p.IsPrimary
                }).ToList() ?? new()
            };

        // -------------------------- Cleanup -----------------------------------------
        public void Dispose()
        {
            _eventBus.Unsubscribe(_onCreated);
            _eventBus.Unsubscribe(_onUpdated);
            _eventBus.Unsubscribe(_onDeleted);
            _eventBus.Unsubscribe(_onDeletedAll);
        }
    }
}
