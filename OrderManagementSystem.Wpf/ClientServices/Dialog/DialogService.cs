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
            // ── Step 1: marshal to the UI thread BEFORE acquiring the gate ────
            //
            // FIX (DEADLOCK): the original code acquired the semaphore first,
            // then checked the dispatcher. When called from a non-UI thread:
            //
            //   outer call  → WaitAsync()          gate: 1 → 0  (acquired)
            //   outer call  → InvokeAsync(ShowDialogCoreAsync)
            //   inner call  → WaitAsync()          gate: 0 → suspends (waiting for outer)
            //   outer call  → awaits InvokeAsync   (waiting for inner)
            //   ── neither can proceed → permanent deadlock ──
            //
            // Fix: perform the dispatcher check (and re-marshal if needed) BEFORE
            // touching the semaphore. The re-marshalled call then runs entirely on
            // the UI thread and acquires + releases the gate cleanly by itself.
            // The outer (non-UI-thread) call returns the inner call's result
            // without ever touching the gate.

            if (!System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                // 'await await': InvokeAsync returns DispatcherOperation<Task<bool>>;
                // the first await unwraps to Task<bool>; the second await gets bool.
                return await await System.Windows.Application.Current.Dispatcher.InvokeAsync(
                    () => ShowDialogCoreAsync(message, title, isConfirmation));
            }

            // ── Step 2: acquire gate (we are now guaranteed to be on the UI thread) ─
            await _gate.WaitAsync();

            DialogViewModel? dialogViewModel = null;
            try
            {
                dialogViewModel = new DialogViewModel(
                    title: title ?? string.Empty,
                    message: message ?? string.Empty,
                    isConfirmation: isConfirmation);

                _mainViewModel.CurrentDialog = dialogViewModel;
                _mainViewModel.IsDialogVisible = true;

                // Suspend until the user presses a button (or Cancel() is called
                // externally, e.g. during application shutdown).
                return await dialogViewModel.DialogTask;
            }
            finally
            {
                // Guaranteed to run even if DialogTask faults or is cancelled,
                // so the overlay is never left permanently on screen.
                _mainViewModel.IsDialogVisible = false;
                _mainViewModel.CurrentDialog = null;

                _gate.Release();
            }
        }

        // ── Shutdown support ──────────────────────────────────────────────────

        /// <summary>
        /// Dismisses any currently open dialog as cancelled.
        /// Call this from <c>App.OnExit</c> before disposing the DI container
        /// so that any ViewModel awaiting a dialog result is unblocked cleanly
        /// rather than hanging indefinitely.
        /// </summary>
        public void DismissAll()
        {
            // If a dialog is visible, cancel it so its awaiter unblocks.
            if (_mainViewModel.CurrentDialog is { } dialog)
                dialog.Cancel();
        }
    }
}
