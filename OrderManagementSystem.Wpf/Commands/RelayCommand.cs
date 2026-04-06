using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace OrderManagementSystem.Wpf.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Func<object?, bool>? _canExecute;

        // Constructor 
        public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // CanExecute يطلق عند تغيير الحالة
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        // CanExecute
        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute(parameter);

        // Execute
        public void Execute(object? parameter) => _execute(parameter);

        // استدعيها يدويا لاجبار ال UI على اعادة التحقق 
        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }
}
