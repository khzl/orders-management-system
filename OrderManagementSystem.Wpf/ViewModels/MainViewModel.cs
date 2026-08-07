using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using OrderManagementSystem.Wpf.ClientService.Navigation;
using OrderManagementSystem.Wpf.Commands;
using OrderManagementSystem.Wpf.Helper;
using OrderManagementSystem.Wpf.ViewModels.Customers;
using OrderManagementSystem.Wpf.ViewModels.Orders;
using OrderManagementSystem.Wpf.ViewModels.Products;
using OrderManagementSystem.Wpf.ViewModels.Reports;

namespace OrderManagementSystem.Wpf.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // ------------- Dependencies -----------------------------------------------
        private readonly INavigationService _navigationService;

        // ---------- Dialog overlay Properties -------------------------------------
        private bool _isDialogVisible;
        public bool IsDialogVisible
        {
            get => _isDialogVisible;
            set
            {
                _isDialogVisible = value;
                OnPropertyChanged(nameof(IsDialogVisible));
            }
        }

        private DialogViewModel? _currentDialog;
        public DialogViewModel? CurrentDialog
        {
            get => _currentDialog;
            set
            {
                _currentDialog = value;
                OnPropertyChanged(nameof(CurrentDialog));
            }
        }

        // ----------- Exposed Service (bound by ContentControl in MainWindow) --------------------------
        public INavigationService NavigationService => _navigationService;

        // ----------- Navigation Commands --------------------------------------------------------------
        public ICommand GoToDashboardCommand { get; } // ReadOnly
        public ICommand GoToCustomersCommand { get; } // ReadOnly
        public ICommand GoToOrdersCommand { get; } // ReadOnly
        public ICommand GoToProductsCommand { get; } // ReadOnly
        public ICommand GoToReportsCommand { get; } // ReadOnly
        public ICommand GoToSettingsCommand { get; } // ReadOnly
        public ICommand AddOrderCommand { get; } // ReadOnly

        // --------------------- Constructor ------------------------------------------------------------
        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService; // Injection 

            // Navigate Between Screens In Sidebar 
            GoToDashboardCommand = new RelayCommand(_ => _navigationService.NavigateTo<DashboardViewModel>());
            GoToCustomersCommand = new RelayCommand(_ => _navigationService.NavigateTo<CustomersViewModel>());
            GoToOrdersCommand = new RelayCommand(_ => _navigationService.NavigateTo<OrdersViewModel>());
            GoToProductsCommand = new RelayCommand(_ => _navigationService.NavigateTo<ProductsViewModel>());
            GoToReportsCommand = new RelayCommand(_ => _navigationService.NavigateTo<ReportsViewModel>());
            GoToSettingsCommand = new RelayCommand(_ => _navigationService.NavigateTo<SettingsViewModel>());

            // Navigates Directly To The Add-Order Screen From The Header toolBar
            AddOrderCommand = new RelayCommand(_ => _navigationService.NavigateTo<AddOrderViewModel>());

            // Open The Default Screen on launch
            _navigationService.NavigateTo<DashboardViewModel>();
        }

    }
}
