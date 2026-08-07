using OrderManagementSystem.Application.Interfaces;
using OrderManagementSystem.Wpf.ClientService.Navigation;
using OrderManagementSystem.Wpf.Commands;
using OrderManagementSystem.Wpf.Helper;
using OrderManagementSystem.Wpf.ViewModels.Orders;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace OrderManagementSystem.Wpf.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        // ------------ Dependencies --------------
        private readonly IOrderService _orderService;
        private readonly ICustomerService _customerService;
        private readonly INavigationService _navigationService;

        // ------------ Collections ---------------
        public ObservableCollection<md_RecentOrderRow> RecentOrders { get; } = new();

        // ------------ Status ------------------
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        // ------------ Summary Stat card properties ---------------
        // Service layer and updated without touching the view 

        private string _totalRevenue = "-";
        public string TotalRevenue
        {
            get => _totalRevenue;
            set
            {
                _totalRevenue = value;
                OnPropertyChanged();
            }
        }

        private string _totalRevenueTrend = "-";
        public string TotalRevenueTrend
        {
            get => _totalRevenueTrend;
            set
            {
                _totalRevenueTrend = value;
                OnPropertyChanged();
            }
        }

        private string _cancelledOrders = "-";
        public string CancelledOrders
        {
            get => _cancelledOrders;
            set
            {
                _cancelledOrders = value;
                OnPropertyChanged();
            }
        }

        private string _cancelledOrdersTrend = "-";
        public string CancelledOrdersTrend
        {
            get => _cancelledOrdersTrend;
            set
            {
                _cancelledOrdersTrend = value;
                OnPropertyChanged();
            }
        }

        private string _activeCustomers = "-";
        public string ActiveCustomers
        {
            get => _activeCustomers;
            set
            {
                _activeCustomers = value;
                OnPropertyChanged();
            }
        }

        private string _activeCustomersTrend = "-";
        public string ActiveCustomersTrend
        {
            get => _activeCustomersTrend;
            set
            {
                _activeCustomersTrend = value;
                OnPropertyChanged();
            }
        }

        private string _pendingShipments = "-";
        public string PendingShipments
        {
            get => _pendingShipments;
            set
            {
                _pendingShipments = value;
                OnPropertyChanged();
            }
        }

        private string _pendingShipmentsTrend = "-";
        public string PendingShipmentsTrend
        {
            get => _pendingShipmentsTrend;
            set
            {
                _pendingShipmentsTrend = value;
                OnPropertyChanged();
            }
        }

        // ------------ Commands -------------------
        public ICommand LoadCommand { get; }
        public ICommand ViewAllOrdersCommand { get; }

        // ------------ Constructor -----------------
        public DashboardViewModel(
            IOrderService orderService,
            ICustomerService customerService,
            INavigationService navigationService)
        {
            _orderService = orderService;
            _customerService = customerService;
            _navigationService = navigationService;

            LoadCommand = new AsyncRelayCommand(
                async _ => await LoadDataAsync(),
                _ => !IsLoading);

            ViewAllOrdersCommand = new RelayCommand(
                _ => _navigationService.NavigateTo<OrdersViewModel>(),
                _ => !IsLoading);

            _ = LoadDataAsync();
        }

        // ------------ Data Loading -----------------
        private async Task LoadDataAsync()
        {
            if (IsLoading)
                return;

            ErrorMessage = null;
            IsLoading = true;

            try
            {
                await Task.WhenAll(
                    LoadSummaryStatsAsync(),
                    LoadRecentOrdersAsync());
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed To Load dashboard Data: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadSummaryStatsAsync()
        {
            await Task.Delay(1);

            TotalRevenue = "$12,840";
            TotalRevenueTrend = "+5.4%";

            CancelledOrders = "23";
            CancelledOrdersTrend = "-2.1%";

            ActiveCustomers = "1,456";
            ActiveCustomersTrend = "+5.4%";

            PendingShipments = "7";
            PendingShipmentsTrend = "+6.7%";
        }

        private async Task LoadRecentOrdersAsync()
        {
            await Task.Delay(1);

            var rows = new List<md_RecentOrderRow>
            {
                new md_RecentOrderRow 
                { 
                    OrderId = "ORD12345",
                    CustomerName = "John Doe",
                    OrderDate = DateTime.Now.AddDays(-1),
                    TotalAmount = 250.00m, 
                    Status = "Shipped" 
                },
                new md_RecentOrderRow 
                { 
                    OrderId = "ORD12346",
                    CustomerName = "Jane Smith",
                    OrderDate = DateTime.Now.AddDays(-2),
                    TotalAmount = 150.00m,
                    Status = "Pending" 
                },
                new md_RecentOrderRow 
                { 
                    OrderId = "ORD12347",
                    CustomerName = "Alice Johnson",
                    OrderDate = DateTime.Now.AddDays(-3),
                    TotalAmount = 300.00m,
                    Status = "Cancelled"
                },
                new md_RecentOrderRow 
                { 
                    OrderId = "ORD12348",
                    CustomerName = "Bob Brown",
                    OrderDate = DateTime.Now.AddDays(-4),
                    TotalAmount = 450.00m,
                    Status = "Shipped" 
                },
                new md_RecentOrderRow 
                { 
                    OrderId = "ORD12349",
                    CustomerName = "Charlie Davis", 
                    OrderDate = DateTime.Now.AddDays(-5),
                    TotalAmount = 200.00m, 
                    Status = "Pending" 
                },
            };

            RecentOrders.Clear();
            foreach (var row in rows)
                RecentOrders.Add(row);
        }


    }
}
