using Microsoft.Extensions.DependencyInjection;
using OrderManagementSystem.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace OrderManagementSystem.Wpf.ClientService.Dialog
{
    /// <summary>
    /// Shows custom WPF overlay dialogs by writing into <see cref="MainViewModel"/>.
    /// A semaphore serialises concurrent calls so no two dialogs fight over the same slot.
    /// </summary>
    public class DialogService : IDialogService
    {
        private readonly MainViewModel _mainViewModel;

        /// <summary>
        /// Ensures at most one dialog is open at a time.
        /// If a second call arrives while a dialog is open, it waits in the queue.
        /// </summary>
        private readonly SemaphoreSlim _gate = new(initialCount: 1, maxCount: 1);

        // ── Constructor ──────────────────────────────────────────────────────
        public DialogService(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel
                ?? throw new ArgumentNullException(nameof(mainViewModel));
        }

        // ── Public API ───────────────────────────────────────────────────────

        /// <inheritdoc/>
        public Task ShowInfoAsync(string message, string title = "Info") =>
            ShowDialogCoreAsync(message, title, isConfirmation: false);

        /// <inheritdoc/>
        public Task ShowErrorAsync(string message, string title = "Error") =>
            ShowDialogCoreAsync(message, title, isConfirmation: false);

        /// <inheritdoc/>
        public Task<bool> ShowConfirmationAsync(string message, string title = "Confirm") =>
            ShowDialogCoreAsync(message, title, isConfirmation: true);

        // ── Core implementation ───────────────────────────────────────────────

        /// <summary>
        /// All public methods funnel through here.
        /// Serialises calls, dispatches to the UI thread, and guarantees cleanup.
        /// </summary>
        private async Task<bool> ShowDialogCoreAsync(
            string message,
            string title,
            bool isConfirmation)
        {
            // One dialog at a time — callers queue here if another is already open
            await _gate.WaitAsync();

            try
            {
                // WPF controls must be touched on the UI thread.
                // If a ViewModel called us after a ConfigureAwait(false), we re-marshal.
                if (!System.Windows.Application.Current.Dispatcher.CheckAccess())
                {
                    return await await System.Windows.Application.Current.Dispatcher.InvokeAsync(
                        () => ShowDialogCoreAsync(message, title, isConfirmation));
                }

                var dialogViewModel = new DialogViewModel(
                    title: title ?? string.Empty,
                    message: message ?? string.Empty,
                    isConfirmation);

                _mainViewModel.CurrentDialog = dialogViewModel;
                _mainViewModel.IsDialogVisible = true;

                try
                {
                    // Suspends here until the user presses OK / Yes / No
                    return await dialogViewModel.DialogTask;
                }
                finally
                {
                    // Always runs — even when DialogTask faults or is cancelled —
                    // so the overlay never gets permanently stuck on screen
                    _mainViewModel.IsDialogVisible = false;
                    _mainViewModel.CurrentDialog = null;
                }
            }
            finally
            {
                _gate.Release();
            }
        }

    }
}
