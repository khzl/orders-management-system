using Microsoft.Extensions.DependencyInjection;
using OrderManagementSystem.Wpf.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace OrderManagementSystem.Wpf.ClientService.Dialog
{
    public class DialogService : IDialogService
    {
        // private field
        private readonly MainViewModel _mainViewModel;

        // public Constructor (Constructor Injection)
        public DialogService(MainViewModel mainViewModel)
        {
            _mainViewModel = mainViewModel; // Injection
        }

        public async Task ShowMessage(string message, string title = "Info")
        {
            var dialogViewModel = new DialogViewModel(title, message, false);

            _mainViewModel.CurrentDialog = dialogViewModel;
            _mainViewModel.IsDialogVisible = true;

            await dialogViewModel.DialogTask; // Wait User Enter OK

            _mainViewModel.IsDialogVisible = false;
            _mainViewModel.CurrentDialog = null;
        }

        public async Task<bool> ShowConfirmation(string message, string title = "Confirm")
        {
            var dialogViewModel = new DialogViewModel(title, message);

            _mainViewModel.CurrentDialog = dialogViewModel;
            _mainViewModel.IsDialogVisible = true;

            var result = await dialogViewModel.DialogTask;

            _mainViewModel.IsDialogVisible = false;
            _mainViewModel.CurrentDialog = null;

            return result; // return true or false 
        }

    }
}
