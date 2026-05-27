using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Wpf.ClientService.Dialog
{
    /// <summary>
    /// Contract for showing non-blocking, custom WPF dialogs from any ViewModel.
    /// All methods are awaitable. Call sites should always use 'await'.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>Shows an informational message with a single OK button.</summary>
        public Task ShowInfoAsync(string message, string title = "Info");

        /// <summary>Shows an error message with a single OK button.</summary>
        public Task ShowErrorAsync(string message, string title = "Error");

        /// <summary>
        /// Shows a Yes / No confirmation dialog.
        /// Returns <c>true</c> if the user confirmed, <c>false</c> otherwise.
        /// </summary>
        public Task<bool> ShowConfirmationAsync(string message, string title = "Confirm");
    }
}
