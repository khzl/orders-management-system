using System;
using System.Collections.Generic;
using System.Text;
using OrderManagementSystem.Wpf.Helper;

namespace OrderManagementSystem.Wpf.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        // private field for CurrentViewModel
        private object? _currentViewModel;

        // public Property
        public object? CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }

        // Constructor
        public MainViewModel()
        {

            CurrentViewModel = new DashboardViewModel();
        }
    }
}
