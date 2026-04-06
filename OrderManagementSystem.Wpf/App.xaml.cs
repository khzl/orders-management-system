using Microsoft.Extensions.DependencyInjection;
using OrderManagementSystem.Application;
using OrderManagementSystem.Wpf.Services.Dialog;
using System.Configuration;
using System.Data;
using System.Windows;
using Applications = System.Windows.Application; // Solution Here
using OrderManagementSystem.Wpf.ViewModels;
using OrderManagementSystem.Wpf.Services;
using OrderManagementSystem.Wpf.Services.Navigation;

namespace OrderManagementSystem.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Applications
    {
        //public static IServiceProvider? ServiceProvider { get; private set; }

        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    base.OnStartup(e);

        //    // dialogService
        //    var dialogService = new DialogService();

        //    // Create Factory  حسب نوع ViewModel
        //    var navigationService = new NavigationService(viewModelType =>
        //    {
        //        if (viewModelType == typeof(DashboardViewModel))
        //            return new DashboardViewModel();
        //        if (viewModelType == typeof(OrdersViewModel))
        //            return new OrdersViewModel(dialogService); // here Injection
        //        if (viewModelType == typeof(SettingsViewModel))
        //            return new SettingsViewModel();

        //        throw new InvalidOperationException($"Unknown ViewModel: {viewModelType.Name}");
        //    });

        //    var mainViewModel = new MainViewModel(navigationService);

        //    var mainWindow = new MainWindow
        //    {
        //        DataContext = mainViewModel // هنا يصير الربط
        //    };

        //    mainWindow.Show();

        //}

    }

}
