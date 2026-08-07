using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using OrderManagementSystem.Wpf.Commands;
using OrderManagementSystem.Wpf.Helper;

namespace OrderManagementSystem.Wpf.ViewModels
{
    public class DialogViewModel : BaseViewModel 
    {
        // ------------- Display properties -------------
        public string Title { get; }
        public string Message { get; }
        public bool IsConfirmation { get; }

        // ------------- Commands --------------------
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }

        // ----------- Completion Source -------------
        private readonly TaskCompletionSource<bool> _tcs;

        // ------------- Constructor ---------------------
        public DialogViewModel(string title,string message, bool isConfirmation = true)
        {
            Title = title;
            Message = message;
            IsConfirmation = isConfirmation;

            _tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            ConfirmCommand = new RelayCommand(_ => _tcs.TrySetResult(true));
            CancelCommand = new RelayCommand(_ => _tcs.TrySetResult(false));
        }

        // ------------ Awaitable result -----------------
        public Task<bool> DialogTask => _tcs.Task;

        /// <summary>
        /// Cancels the dialog, unblocking any awaiter.
        /// Called during application shutdown to prevent indefinite hangs.
        /// </summary>
        public void Cancel() => _tcs.TrySetResult(false);
    }
}
