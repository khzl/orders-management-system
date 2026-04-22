using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Wpf.ClientService.Dialog
{
    public interface IDialogService
    {
        // for public message 
        public Task ShowMessage(string message, string title = "Info");
        public Task<bool> ShowConfirmation(string message, string title = "Confirm");
    }
}
