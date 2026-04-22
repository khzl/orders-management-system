using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using OrderManagementSystem.Wpf.Commands;
using OrderManagementSystem.Wpf.Helper;

namespace OrderManagementSystem.Wpf.ViewModels
{
    // وسيطاً يحمل البيانات (العنوان، الرسالة) والأوامر (تأكيد، إلغاء).
    public class DialogViewModel : BaseViewModel 
    {
        // Properties 
        private string? _title;
        public string? Title 
        { 
            get => _title;
            set
            {
                _title = value;
                OnPropertyChanged(nameof(Title));
            } 
        }

        private string? _message;
        public string? Message 
        { 
            get =>  _message;
            set
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }

        private bool _isConfirmation;
        public bool IsConfirmation
        {
            get => _isConfirmation;
            set
            {
                _isConfirmation = value;
                OnPropertyChanged(nameof(IsConfirmation));
            }
        }


        private readonly TaskCompletionSource<bool> _taskCompletionSource = new();

        // Commands
        public ICommand? ConfirmCommand { get; } // ReadOnly 
        public ICommand? CancelCommand { get; } // ReadOnly

        // public Constructor (Constructor Injections)
        public DialogViewModel(string title,string message, bool isConfirmation = true)
        {
            Title = title;
            Message = message;
            IsConfirmation = isConfirmation;
            _taskCompletionSource = new TaskCompletionSource<bool>();

            ConfirmCommand = new RelayCommand(_ => _taskCompletionSource.SetResult(true));
            CancelCommand = new RelayCommand(_ => _taskCompletionSource.SetResult(false));
        }

        // هذه المهمة ننتظرها في الـ Service لتعرف ماذا اختار المستخدم
        public Task<bool> DialogTask => _taskCompletionSource.Task;
    }
}
