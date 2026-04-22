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

namespace OrderManagementSystem.Wpf.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // private field 
        private readonly INavigationService _navigationService;

        // Two Properties to Control for Overlay 
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

        // public Property 
        public INavigationService NavigationService => _navigationService;

        // Commands 
        public ICommand? GoToDashboardCommand { get; } // ReadOnly
        public ICommand? GoToCustomersCommand { get; } // ReadOnly
        public ICommand? GoToOrdersCommand { get; } // ReadOnly
        public ICommand? GoToProductsCommand { get; } // ReadOnly

        // public Constructor (Constructor Injection)
        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService; // Injection 

            GoToDashboardCommand = new RelayCommand(_ => _navigationService.NavigateTo<DashboardViewModel>());
            GoToCustomersCommand = new RelayCommand(_ => _navigationService.NavigateTo<CustomersViewModel>());
            GoToOrdersCommand = new RelayCommand(_ => _navigationService.NavigateTo<OrdersViewModel>());
            GoToProductsCommand = new RelayCommand(_ => _navigationService.NavigateTo<ProductsViewModel>());

            // Default View Over Open
            _navigationService.NavigateTo<DashboardViewModel>();
        }

        // Helper Method To Open Any Dialog In Any Where 
        public void ShowDialog(string title, string message, Action onConfirm)
        {
            var dialog = new DialogViewModel(title, message, true);

            // When the dialog completes, run the confirmation callback (if confirmed)
            // and hide the overlay. Schedule continuation on the UI sync context.
            dialog.DialogTask.ContinueWith(t =>
            {
                if (t.Result)
                    onConfirm?.Invoke();

                IsDialogVisible = false;
            }, TaskScheduler.FromCurrentSynchronizationContext());

            CurrentDialog = dialog;
            IsDialogVisible = true;
        }

    }
}
