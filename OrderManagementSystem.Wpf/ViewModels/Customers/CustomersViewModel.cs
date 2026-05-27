using OrderManagementSystem.Application.Interfaces;
using OrderManagementSystem.Domain.Entities;
using OrderManagementSystem.Dtos.Customers;
using OrderManagementSystem.Wpf.ClientService.Dialog;
using OrderManagementSystem.Wpf.ClientService.Navigation;
using OrderManagementSystem.Wpf.ClientServices.EvenService;
using OrderManagementSystem.Wpf.Commands;
using OrderManagementSystem.Wpf.Helper;
using OrderManagementSystem.Wpf.Helper.Enums;
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
        // private field
        private readonly ICustomerService _customerService;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly IEventBus _eventBus;

        //(Buffer) list to store data original
        private List<CustomerDto> _allCustomers = new();

        // Handlers نحتفظ بها لاحقا for Unsubscribe 
        private readonly Action<CustomerCreatedEvent> _onCreated;
        private readonly Action<CustomerUpdatedEvent> _onUpdated;
        private readonly Action<CustomerDeletedEvent> _onDeleted;
        private readonly Action<CustomerDeletedAllEvent> _onDeletedAll;


        // property List Collection For Customer list linked for DataGrid
        public RangeObservableCollection<CustomerDto> Customers { get; set; } = new();

        // Properties Binding 
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

        // Search Text Property 
        private string? _searchText;
        public string? SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged(nameof(SearchText));
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading == value)
                    return; // تاكد ان القيمة تغيرت لتجنب التحديثات الزائدة 
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        private bool _isDetailsExpanded;
        public bool IsDetailsExpanded
        {
            get => _isDetailsExpanded;
            set
            {
                _isDetailsExpanded = value;
                OnPropertyChanged(nameof(IsDetailsExpanded));
            }
        }

        // Pagination Properties 
        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (_currentPage == value || value < 1)
                    return; // لا تسمح بأن تكون الصفحة اقل من 1

                _currentPage = value;
                OnPropertyChanged(nameof(CurrentPage));
                // جلب البيانات تلقائياً فور تغير الصفحة القادمة من الـ Component
                _ = LoadData();
            }
        }

        private int _totalPages = 25; // لازم تنطيه مجموع الصفحات بعدد مناسب حتى يكدر يعرض 
        public int TotalPages
        {
            get => _totalPages;
            set
            {
                // لا تسمح بأن تكون الصفحات أفل من 1 حتى لو لم تكن هناك بيانات 
                if (_totalPages != value)
                {
                    _totalPages = value;
                    OnPropertyChanged(nameof(TotalPages));
                }
            }
        }

        private int _pageSize = 10; // Number Of Items Per Page
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

        // Selected Search Type property 
        // Property To Store Selected Search Type 
        private en_CustomerSearchType? _selectedSearchType = en_CustomerSearchType.All;
        public en_CustomerSearchType? SelectedSearchType
        {
            get => _selectedSearchType;
            set
            {
                _selectedSearchType = value;
                OnPropertyChanged(nameof(SelectedSearchType));
            }
        }

        // قائمة الأنواع لعرضها في الـ ComboBox
        public IEnumerable<en_CustomerSearchType> SearchTypes =>
            Enum.GetValues(typeof(en_CustomerSearchType)).Cast<en_CustomerSearchType>();


        // Command
        public ICommand? LoadCommand { get; }
        public ICommand? AddCommand { get; }
        public ICommand? DeleteCommand { get; } // Delete One (in Row)
        public ICommand? DeleteAllCommand { get; } // Delete All (in Top Button)
        public ICommand? EditCommand { get; }
        public ICommand? NavigateToPhonesCommand { get; } // Go To PhonesView

        // Constructor 
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

            // ─────────────────────────────────────────
            // Subscribe — كل Event يعمل Reload تلقائي
            // ─────────────────────────────────────────
            _onCreated = async _ => await LoadData();
            _onUpdated = async _ => await LoadData();
            _onDeleted = async _ => await LoadData();
            _onDeletedAll = _ => App.Current.Dispatcher.Invoke(() =>
            {
                _allCustomers.Clear();
            });

            _eventBus.Subscribe(_onCreated);
            _eventBus.Subscribe(_onUpdated);
            _eventBus.Subscribe(_onDeleted);
            _eventBus.Subscribe(_onDeletedAll);

            // Commands 
            LoadCommand = new AsyncRelayCommand(async _ => await LoadData());
            AddCommand = new AsyncRelayCommand(async _ => await GoToAddCustomer());

            DeleteCommand = new AsyncRelayCommand(async obj =>
            {
                if (obj is CustomerDto customerDto)
                {
                    await DeleteCustomer(customerDto);
                }
            });

            DeleteAllCommand = new AsyncRelayCommand(async _ => await DeleteAllCustomers());

            EditCommand = new AsyncRelayCommand(async obj =>
            {
                if (obj is CustomerDto customerDto)
                {
                    var updateCustomerDto = new UpdateCustomerDto
                    {
                        CustomerId = customerDto.CustomerId,
                        CustomerName = customerDto.CustomerName,
                        Email = customerDto.Email,
                        Address = customerDto.Address,
                        CustomerPhones = customerDto.Phones.Select(p => new CustomerPhoneDto
                        {
                            PhoneId = p.PhoneId,
                            CustomerId = p.CustomerId,
                            PhoneNumber = p.PhoneNumber,
                            PhoneType = p.PhoneType,
                            IsPrimary = p.IsPrimary
                        }).ToList()
                    };
                    await GoToUpdateCustomer(updateCustomerDto);
                }
            });

            NavigateToPhonesCommand = new AsyncRelayCommand(async obj =>
            {
                if (obj is CustomerDto customerDto)
                {
                    var customerPhoneDto = new CustomerPhoneDto
                    {
                        CustomerId = customerDto.CustomerId
                    };
                    await GoToCustomerPhones(customerPhoneDto);
                }
            });

        }


        // LoadData
        private async Task LoadData()
        {
            if (IsLoading)
                return; // Guard against multiple concurrent loads

            ErrorMessage = null;

            try
            {
                IsLoading = true;

                var result =
                    await _customerService.GetAllAsync(CurrentPage, PageSize);

                if (result.IsSuccess)
                {
                    var pagedData = result.Data;

                    _allCustomers = pagedData?.Data?.ToList() ?? new List<CustomerDto>();

                    Customers.ReplaceRange(_allCustomers);

                    int serverPages = pagedData?.TotalPages ?? 1;
                    TotalPages = serverPages < 1 ? 1 : serverPages;

                    // تحديث العدد الإجمالي إذا كان قادماً من السيرفر
                    TotalCount = pagedData?.TotalCount ?? _allCustomers.Count;
                }
                else
                {
                    ErrorMessage = result.Error;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Navigation To Add CustomerView
        private Task GoToAddCustomer()
        {
            _navigationService.NavigateTo<AddCustomerViewModel>();
            return Task.CompletedTask;
        }

        // Navigation To Update CustomerView 
        private Task GoToUpdateCustomer(UpdateCustomerDto updateCustomerDto)
        {
            if (updateCustomerDto == null)
                return Task.CompletedTask;

            _navigationService.NavigateTo<UpdateCustomerViewModel>(updateCustomerDto);
            return Task.CompletedTask;
        }

        private Task GoToCustomerPhones(CustomerPhoneDto customerPhoneDto)
        {
            _navigationService.NavigateTo<CustomerPhonesViewModel>(customerPhoneDto);
            return Task.CompletedTask;
        }

        // Delete One Customer By Id
        // when delete will be must delete from two list 
        private async Task DeleteCustomer(CustomerDto customerDto)
        {
            var result = MessageBox.Show($"Are you sure you want to delete {customerDto.CustomerName}?",
                                 "Confirm Delete",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                IsLoading = true;
                var response = await _customerService.DeleteAsync(customerDto.CustomerId);

                if (response.IsSuccess)
                {
                    // EventBus يتولى الـ Reload — ما نحتاج نستدعي LoadData يدوياً
                    _eventBus.Publish(new CustomerDeletedEvent(customerDto.CustomerId));
                }
                else
                {
                    ErrorMessage = response.Error;
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Delete All Customers
        private async Task DeleteAllCustomers()
        {
            if (!_allCustomers.Any())
                return;

            var result = MessageBox.Show("CRITICAL: Do you really want to wipe ALL records?",
                                 "Delete All Confirmation",
                                 MessageBoxButton.YesNo,
                                 MessageBoxImage.Error);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                IsLoading = true;
                var response = await _customerService.DeleteAllAsync();

                if (response.IsSuccess)
                {
                    _eventBus.Publish(new CustomerDeletedAllEvent());
                }
                else
                {
                    ErrorMessage = response.Error;
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ─────────────────────────────────────────
        // CLEANUP — Unsubscribe عند إغلاق الـ ViewModel
        // ─────────────────────────────────────────
        public void Dispose()
        {
            _eventBus.Unsubscribe(_onCreated);
            _eventBus.Unsubscribe(_onUpdated);
            _eventBus.Unsubscribe(_onDeleted);
            _eventBus.Unsubscribe(_onDeletedAll);
        }
    }
}
