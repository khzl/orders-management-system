using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Wpf.ClientServices.Navigation
{
    // Primary Interface For Navigation Service 
    public interface Iloadable
    {
        public void Load(object parameter);
    }

    // Generic Interface For Used In ViewModel
    public interface ILoadable<T> : Iloadable
    {
        public void Load(T parameter);

        void Iloadable.Load(object parameter) => Load((T)parameter);
    }
}
