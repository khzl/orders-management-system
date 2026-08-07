using Microsoft.Extensions.Configuration;
using OrderManagementSystem.Wpf.ClientService.Dialog;
using OrderManagementSystem.Wpf.Commands;
using OrderManagementSystem.Wpf.Helper;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Net.Http;

namespace OrderManagementSystem.Wpf.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        // ---------- Validation ----------------
        private static readonly Regex UrlRegex = new(
            @"^https?://.+",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // ----------- Dependencies ----------------
        private readonly IConfiguration _configuration;
        private readonly IDialogService _dialogService;

        // -------- Default Values (Used By Reset Command) ----------------
        private const string DefaultApiUrl = "https://api.example.com";
        private const double DefaultTaxRateValue = 15.0;
        private const string DefaultLanguage = "English";

        // --------- Bound Properties -----------------
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        // Status bar - carries both success and error messages + a severity flag
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

        private bool _statusIsError;
        /// <summary>
        /// True -> status text shown in DangerTextBrush (Save Failed, validation error)
        /// False -> status text shown in successTextBrush (Saved Successfully)
        /// // Bound via DataTrigger in the View..
        /// </summary>
        public bool StatusIsError
        {
            get => _statusIsError;
            set
            {
                _statusIsError = value;
                OnPropertyChanged(nameof(StatusIsError));
            }
        }

        // --------- Appearance & Localisation Settings -----------------

        public IReadOnlyList<string> AvailableLanguages { get; } =
            new List<string> { "English" , "Arabic" , "French" , "German" };

        private string _selectedLanguage = DefaultLanguage;
        public string SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                _selectedLanguage = value;
                OnPropertyChanged(nameof(SelectedLanguage));
                ClearStatus();
            }
        }

        private bool _isDarkMode;
        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                _isDarkMode = value;
                OnPropertyChanged(nameof(IsDarkMode));
                // Apply the theme change immediately so the user sees the result

                // before they click Save..
                ApplyTheme(value);
                ClearStatus();
            }
        }

        // --------- Connectivity -------------------------
        private bool _enableNotifications;
        public bool EnableNotifications
        {
            get => _enableNotifications;
            set
            {
                _enableNotifications = value;
                OnPropertyChanged(nameof(EnableNotifications));
                ClearStatus();
            }
        }

        private string _apiBaseUrl = DefaultApiUrl;
        public string ApiBaseUrl
        {
            get => _apiBaseUrl;
            set
            {
                _apiBaseUrl = value;
                OnPropertyChanged(nameof(ApiBaseUrl));
                ClearStatus();
            }
        }

        // --------- Business Defaults -------------------------
        private string _defaultTaxRate = DefaultTaxRateValue.ToString("F1");
        public string DefaultTaxRate
        {
            get => _defaultTaxRate;
            set
            {
                _defaultTaxRate = value;
                OnPropertyChanged(nameof(DefaultTaxRate));
                ClearStatus();
            }
        }

        // ---------- Commands -----------------
        public ICommand? SaveCommand { get; } // ReadOnly
        public ICommand? ResetCommand { get; } // ReadOnly

        // ---------- Constructor -----------------
        public SettingsViewModel(
            IConfiguration configuration,
            IDialogService dialogService)
        {
            _configuration = configuration;
            _dialogService = dialogService;

            SaveCommand = new AsyncRelayCommand(
                _ =>  SaveAsync(),
                _ => !IsLoading);

            ResetCommand = new AsyncRelayCommand(
                _ =>  ResetToDefaultsAsync(),
                _ => !IsLoading);

            LoadFromConfiguration();
        }

        // ----------- Initialization -----------------
        /// <summary>
        /// seeds the form fields the current <c>IConfiguration</c>
        /// </summary>
        private void LoadFromConfiguration()
        {
            SelectedLanguage = _configuration["Settings:Language"] ?? DefaultLanguage;
            ApiBaseUrl = _configuration["Settings:ApiBaseUrl"] ?? DefaultApiUrl;
            EnableNotifications = bool.TryParse(_configuration["Settings:EnableNotifications"], out var notif) && notif;
            IsDarkMode = bool.TryParse(_configuration["Settings:IsDarkMode"], out var dark) && dark;

            if (double.TryParse(_configuration["Settings:DefaultTaxRate"], out double rate))
                DefaultTaxRate = rate.ToString("F1");
            else
                DefaultTaxRate = DefaultTaxRateValue.ToString("F1");
        }

        // ------------- Save ----------------------------
        private async Task SaveAsync()
        {
            StatusMessage = null;

            if (!ValidateInputs())
                return;

            IsLoading = true;

            try
            {
                // Persist to appsettings.json 
                await PersistSettingsAsync();

                SetStatus("Settings Saved Successfully.", isError: false);
            }
            catch (Exception ex)
            {
                SetStatus($"Save Failed: {ex.Message}", isError: true);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ------------- Reset ------------------------------
        private async Task ResetToDefaultsAsync()
        {
            bool confirmed = await _dialogService.ShowConfirmationAsync(
                "This Will Revert All Settings To Their Factory Defaults. Continue?",
                "Reset To Defaults");

            if (!confirmed)
                return;

            SelectedLanguage = DefaultLanguage;
            ApiBaseUrl = DefaultApiUrl;
            DefaultTaxRate = DefaultTaxRateValue.ToString("F1");
            EnableNotifications = false;
            IsDarkMode = false;

            SetStatus("Settings Reset To Defaults. Click Save To Persist.", isError: false);
        }

        // ------------- Validation -------------------------
        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(ApiBaseUrl) || !UrlRegex.IsMatch(ApiBaseUrl.Trim()))
            {
                SetStatus("API Base URL Must Start With http:// or https://.", isError: true);
                return false;
            }

            if (!double.TryParse(DefaultTaxRate, out double rate) || rate < 0 || rate > 100)
            {
                SetStatus("Default Tax Rate Must Be a Number Between 0 And 100.", isError: true);
                return false;
            }

            return true;
        }

        // --------------- Dark Mode ------------------------
        /// <summary>
        /// Swaps the top-level theme ResourceDictionary at runtime.
        /// Expects two files in /Themes/:
        ///   LightTheme.xaml  — (or Colors.xaml as the light default)
        ///   DarkTheme.xaml   — override brushes for dark mode
        /// The convention: the last entry in App.Current.Resources.MergedDictionaries
        /// whose Source contains "Theme" is replaced.
        /// </summary>
        private static void ApplyTheme(bool isDark)
        {
            var mergedDicts = System.Windows.Application.Current.Resources.MergedDictionaries;

            // Remove any existing theme dictionary
            var existing = mergedDicts
                .FirstOrDefault(d => d.Source?.OriginalString.Contains("Theme") == true);

            if (existing != null)
                mergedDicts.Remove(existing);

            var themePath = isDark
                ? "/Themes/DarkTheme.xaml"
                : "/Themes/Colors.xaml";

            mergedDicts.Add(new ResourceDictionary
            {
                Source = new Uri(themePath, UriKind.Relative)
            });
        }

        // ── Persistence stub ─────────────────────────────────────────────────

        /// <summary>
        /// Writes the current settings to a writable store.
        /// Replace the body with your chosen persistence mechanism
        /// (JSON write-back, SQLite, user Properties.Settings, etc.).
        /// The Task.Delay simulates the async I/O round-trip during development.
        /// </summary>
        private async Task PersistSettingsAsync()
        {
            // TODO: replace with real persistence
            await Task.Delay(300);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private void SetStatus(string message, bool isError)
        {
            StatusMessage = message;
            StatusIsError = isError;
        }

        private void ClearStatus()
        {
            StatusMessage = null;
            StatusIsError = false;
        }

    }
}
