using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace OrderManagementSystem.Wpf.ClientService.Dialog
{
    public class DialogService : IDialogService
    {

        public void ShowMessage(string message, string title = "Info")
        {
            MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public bool ShowConfirmation(string message, string title = "Confirm")
        {
            var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

    }
}
