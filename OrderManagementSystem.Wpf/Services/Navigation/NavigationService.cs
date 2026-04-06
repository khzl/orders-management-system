using OrderManagementSystem.Wpf.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Wpf.Services.Navigation
{
    public class NavigationService : BaseViewModel , INavigationService
    {
        // Factory To Create ViewModels
        private readonly Func<Type, object?> _viewModelFactory;

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
        public NavigationService(Func<Type, object?> viewModelFactory)
        {
            _viewModelFactory = viewModelFactory;
        }

        public void NavigateTo<TViewModel>() where TViewModel : class
        {
            NavigateTo<TViewModel>(null);
        }

        public void NavigateTo<TViewModel>(object? parameter) where TViewModel : class
        {
            var viewModel = _viewModelFactory(typeof(TViewModel)) as TViewModel
                ?? throw new InvalidOperationException($"Cannot Create ViewModel of Type {typeof(TViewModel).Name}");

            CurrentViewModel = viewModel;

            // can upload Event here For Notifiaction For View 
            // OnCurrentViewModelChanged();
        }

        public void Navigate(string url)
        {

        }
    }
}
