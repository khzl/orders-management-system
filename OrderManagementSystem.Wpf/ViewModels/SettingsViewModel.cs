using OrderManagementSystem.Wpf.Commands;
using OrderManagementSystem.Wpf.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;

namespace OrderManagementSystem.Wpf.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        // Settings UI And System Logic Here 

        // Properties For Binding To UI Controls 
        private bool _isDarkMode;
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                _isDarkMode = value;
                OnPropertyChanged(nameof(IsDarkMode));
            }
        }

        private string? _selectedLanguage;
        public string? SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                _selectedLanguage = value;
                OnPropertyChanged(nameof(SelectedLanguage));
            }
        }

        public ObservableCollection<string> AvailableLanguages { get; } = new() { "English", "العربية" };

        private bool _enableNotifications = true;
        public bool EnableNotifications
        {
            get => _enableNotifications;
            set
            {
                _enableNotifications = value;
                OnPropertyChanged(nameof(EnableNotifications));
            }
        }

        // Settings Logic Business Logic Here (e.g. Save Settings , Load Settings, etc.)
        private decimal _defaultTaxRate = 15.0m;
        public decimal DefaultTaxRate
        {
            get => _defaultTaxRate;
            set
            {
                _defaultTaxRate = value;
                OnPropertyChanged(nameof(DefaultTaxRate));
            }
        }

        private string _apiBaseUrl = "https://localhost:5001/api/";
        public string ApiBaseUrl
        {
            get => _apiBaseUrl;
            set
            {
                _apiBaseUrl = value;
                OnPropertyChanged(nameof(ApiBaseUrl));
            }
        }

        // Ui State logic Here
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading == value)
                    return;
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
                NotifyCommands();
            }
        }

        private string? _statusMessage;
        public string? StatusMessage
        {
            get => _statusMessage;
            set
            {
                _statusMessage = value;
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        // Commands For Save And Reset Settings
        public ICommand? SaveCommand { get; } // ReadOnly
        public ICommand? ResetCommand { get; } // ReadOnly

        // Constructor
        public SettingsViewModel()
        {
            // يفضل لاحقاً حقن ISettingsService هنا لجلب وحفظ الإعدادات من قاعدة البيانات أو ملف Local

            SaveCommand = new AsyncRelayCommand(async _ => await SaveSettingsAsync(), _ => !IsLoading);
            ResetCommand = new AsyncRelayCommand(async _ => await ResetSettingsAsync(), _ => !IsLoading);

            LoadInitialSettings();
        }

        private void LoadInitialSettings()
        {
            // يتم جلب الإعدادات الحالية هنا
            SelectedLanguage = "English";
            IsDarkMode = false;
        }

        private async Task SaveSettingsAsync()
        {
            StatusMessage = null;
            IsLoading = true;

            try
            {
                // محاكاة لعملية الحفظ في الـ API أو ملف الإعدادات
                await Task.Delay(1500);

                StatusMessage = "Settings saved successfully!";

                // هنا ممكن تستدعي EventBus لإعلام باقي الواجهات بتغير اللغة أو الثيم
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving settings: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ResetSettingsAsync()
        {
            IsLoading = true;
            await Task.Delay(500); // محاكاة

            IsDarkMode = false;
            SelectedLanguage = "English";
            EnableNotifications = true;
            DefaultTaxRate = 15.0m;
            ApiBaseUrl = "https://localhost:5001/api/";

            StatusMessage = "Settings reset to default.";
            IsLoading = false;
        }

        // NotifyCommands to update the state of save and reset buttons based on changes in settings properties 
        private void NotifyCommands()
        {
            (SaveCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            (ResetCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
        }
    }
}
