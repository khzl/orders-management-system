using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace OrderManagementSystem.Wpf.Helper
{
    public class BaseViewModel
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected void OnPropertyChanged() =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));

    }
}
