using System;
using System.Collections.Generic;
using System.Text;

namespace OrderManagementSystem.Wpf.Services.Dialog
{
    public interface IDialogService
    {
        // for public message 
        public void ShowMessage(string message, string title = "Info");
        public bool ShowConfirmation(string message, string title = "Confirm");
    }
}
