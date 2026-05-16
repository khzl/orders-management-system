using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrderManagementSystem.Application;
using OrderManagementSystem.Infrastructure;
using OrderManagementSystem.Infrastructure.DBContext;
using OrderManagementSystem.Wpf.ClientService.Dialog;
using OrderManagementSystem.Wpf.ClientService.Navigation;
using OrderManagementSystem.Wpf.ClientServices.EvenService;
using OrderManagementSystem.Wpf.Helper;
using OrderManagementSystem.Wpf.ViewModels;
using OrderManagementSystem.Wpf.ViewModels.Customers;
using OrderManagementSystem.Wpf.ViewModels.Orders;
using OrderManagementSystem.Wpf.ViewModels.Products;
using OrderManagementSystem.Wpf.ViewModels.Reports;
using System.IO;
using System.Windows;
using Applications = System.Windows.Application; // Solution Here

namespace OrderManagementSystem.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Applications
    {
        public static IServiceProvider? ServiceProvider { get; private set; }
        public IConfiguration Configuration { get; private set; } = default!;

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                base.OnStartup(e);

                var builder = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

                Configuration = builder.Build();

                var services = new ServiceCollection();

                ConfigureServices(services);

                ServiceProvider = services.BuildServiceProvider();

                // الـ DI سيتكفل بجلب الـ MainWindow ومعه الـ MainViewModel تلقائياً
                var mainWWindow = ServiceProvider.GetRequiredService<MainWindow>();
                mainWWindow.Show();
            }
            catch (Exception ex)
            {
                // هذا السطر سيخبرك بالضبط ما هو الـ ViewModel أو الخدمة المفقودة
                MessageBox.Show($"خطأ في تشغيل التطبيق:\n{ex.Message}\n\nالتفاصيل الداخيلة: {ex.InnerException?.Message}", 
                    "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Applications.Current.Shutdown();
            }
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // register the IConfiguration instance (non-nullable)
            services.AddSingleton<IConfiguration>(Configuration);

            // Navigation build Factory ViewModel From DI
            services.AddSingleton<INavigationService, NavigationService>(provider =>
            new NavigationService(type =>
            {
                try
                {
                    return (BaseViewModel)provider.GetRequiredService(type);
                }
                catch(Exception ex)
                {
                    // سيعطيك اسم الخدمة المفقودة بدقة في الـ Debugger
                    System.Diagnostics.Debug.WriteLine($"Error resolving {type.Name}: {ex.Message}");
                    throw;
                }
            }));

            // call DataAccess (Infrastructure)
            // this line register DbContext , IDbconnection , Repositories
            services.AddDataLayer(Configuration);

            // call application Layer (Application)
            // this line register Services
            services.AddBusinessLayer();

            // Dialog Service Register Here (DI)
            services.AddSingleton<IDialogService, DialogService>();

            // Event Service Register Here (DI)
            services.AddSingleton<IEventBus, EventBus>();

            // ViewModels Registers (DI)
            services.AddTransient<DashboardViewModel>();
            services.AddTransient<DialogViewModel>();

            // Register Customers ViewModel Here 
            services.AddSingleton<CustomersViewModel>();
            services.AddTransient<AddCustomerViewModel>();
            services.AddTransient<UpdateCustomerViewModel>();

            // Register Orders ViewModel Here 
            services.AddSingleton<OrdersViewModel>();

            // Register Products ViewModel Here 
            services.AddSingleton<ProductsViewModel>();

            // Register Reports ViewModel Here 
            services.AddSingleton<ReportsViewModel>();

            // Register Settings ViewModel Here 
            services.AddSingleton<SettingsViewModel>();

            // UI Registers (DI)
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();
        }

    }
}
