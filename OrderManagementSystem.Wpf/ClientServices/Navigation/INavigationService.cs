using OrderManagementSystem.Wpf.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OrderManagementSystem.Wpf.ClientService.Navigation
{
    public interface INavigationService : INotifyPropertyChanged // للتحديث اشعار لتحديث البيانات
    {
        // Current ViewModel To Access ViewModel Current
        public object? CurrentViewModel { get; }

        // NavigationTo<tViewModel> Without Parameter
        public void NavigateTo<TViewModel>() where TViewModel : BaseViewModel;

        // NavigateTo<TViewModel> With parameter Optional 
        public void NavigateTo<TViewModel>(object? parameter) where TViewModel : BaseViewModel;
    }
}
