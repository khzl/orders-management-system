using OrderManagementSystem.Wpf.ClientServices.Navigation;
using OrderManagementSystem.Wpf.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Wpf.ClientService.Navigation
{
    public class NavigationService : BaseViewModel , INavigationService
    {
        // Factory To Create ViewModels
        private readonly Func<Type, BaseViewModel> _viewModelFactory;

        // CurrentViewModel
        private object? _currentViewModel;

        public object? CurrentViewModel
        {
            get => _currentViewModel;
            private set
            {
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel)); // Notification For UI
            }
        }


        // Constructor : pass Factory Function to create ViewModel
        public NavigationService(Func<Type, BaseViewModel> viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
        }

        public void NavigateTo<TViewModel>() where TViewModel : BaseViewModel
        {
            NavigateTo<TViewModel>(null);
        }

        public void NavigateTo<TViewModel>(object? parameter) where TViewModel : BaseViewModel
        {
            var viewModel = _viewModelFactory(typeof(TViewModel)) as TViewModel
               ?? throw new InvalidOperationException($"Cannot Create ViewModel of Type {typeof(TViewModel).Name}");

            // Solution Here 
            if (parameter != null && viewModel is Iloadable loadableViewModel)
            {
                loadableViewModel.Load(parameter);
            }

            CurrentViewModel = viewModel;
        }
    }
}
