using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
using Applications = System.Windows.Application; // // Disambiguates from OrderManagementSystem.Application namespace
using Microsoft.Extensions.Logging.Console;
using Microsoft.Extensions.Logging.Debug;


namespace OrderManagementSystem.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Applications
    {
        public static IServiceProvider? ServiceProvider { get; private set; }
        public IConfiguration Configuration { get; private set; } = default!;
        private ILogger<App>? _logger;

        // ---------------- Startup -----------------------------------------
        protected override void OnStartup(StartupEventArgs e)
        {
            RegisterGlobalExceptionHandlers();

            try
            {
                base.OnStartup(e);

                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false,
                    reloadOnChange: true);

                Configuration = builder.Build();

                var services = new ServiceCollection();

                ConfigureServices(services);

                ServiceProvider = services.BuildServiceProvider();

                _logger = ServiceProvider.GetRequiredService<ILogger<App>>();

                _logger.LogInformation("Application Starting Up....");

                // DI Resolves MainWindow Together With Its MainViewModel Dependency
                var mainWWindow = ServiceProvider.GetRequiredService<MainWindow>();

                mainWWindow.Show();
            }
            catch (Exception ex)
            {
                ShowFatalError("Failed To Start The Application", ex);
                Applications.Current.Shutdown(-1);
            }
        }

        // ------------------- Shutdown -------------------------------
        protected override void OnExit(ExitEventArgs e)
        {
            _logger?.LogInformation("Application Shutting down...");

            (ServiceProvider as IDisposable)?.Dispose();

            base.OnExit(e);
        }

        // --------------------- Global Exception Handling ------------------
        private void RegisterGlobalExceptionHandlers()
        {
            // UI-thread exceptions not already caught by a try/catch
            DispatcherUnhandledException += (_, args) =>
            {
                _logger?.LogError(args.Exception, "Unhandled UI-thread exception...");
                ShowFatalError("An Unexpected error occurred" , args.Exception);
                // mark as handled so the app can keep running rather than crash
                // outright the user can save work and restart if needed
                args.Handled = true;
            };

            // Exceptions on background threads outside the dispatcher 
            AppDomain.CurrentDomain.UnhandledException += (_, args) =>
            {
                var ex = args.ExceptionObject as Exception;
                _logger?.LogCritical(ex, 
                    "Unhandled background-thread exception. IsTerminating = {IsTerminating}",
                    args.IsTerminating);
                ShowFatalError("A critical background error occurred", ex);
            };

            // Exceptions from async Tasks that were never awaited/observed
            TaskScheduler.UnobservedTaskException += (_, args) =>
            {
                _logger?.LogError(args.Exception, "Unobserved Task Exception....");
                // Prevents the finalizer thread from re-throwing and crashing the process
                args.SetObserved();
            };
        }

        // ----------- Show Fatal Error --------------------------
        private void ShowFatalError(string title, Exception? ex)
        {
            string details = ex?.InnerException?.Message is { Length: > 0 } inner
                ? $"{ex.Message}\n\nDetails; {inner}"
                : ex?.Message ?? "No Further Details Available..";

            MessageBox.Show(
                $"{title}:\n{details}",
                "Critical Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }

        // --------------------- Dependency Injection Configuration -----------------
        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(Configuration);

            // Basic Logging: Debug output window (Always) + Console (When Attached)
            services.AddLogging(builder =>
            {
                builder.AddDebug();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Information);
            });

            services.AddSingleton<INavigationService>(provider =>
                new NavigationService(type =>
                {
                    try
                    {
                        return (BaseViewModel)provider.GetRequiredService(type);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine
                        ($"NavigationService Failed To Resolve '{type.Name}': {ex.Message}");
                        throw;
                    }
                }));

            // Data Access Layer (DbContext , Repositories) - Infrastructure project
            services.AddDataLayer(Configuration);

            // Business/service layer - Application project
            services.AddBusinessLayer();

            services.AddSingleton<IDialogService, DialogService>();
            services.AddSingleton<IEventBus, EventBus>();

            // -------------- ViewModels Registration ----------------
            services.AddTransient<DashboardViewModel>();

            // Customers  
            services.AddSingleton<CustomersViewModel>(); // persists list/search/paging state across navigation
            services.AddTransient<AddCustomerViewModel>(); // fresh form every time
            services.AddTransient<UpdateCustomerViewModel>(); // fresh form every time 
            services.AddTransient<CustomerPhonesViewModel>(); // fresh per customer 

            // Orders 
            services.AddSingleton<OrdersViewModel>();
            services.AddTransient<AddOrderViewModel>();

            // Products 
            services.AddSingleton<ProductsViewModel>();

            // Reports 
            services.AddSingleton<ReportsViewModel>();

            // Settings 
            services.AddSingleton<SettingsViewModel>();

            // -------- UI Shell and Main Window ----------------
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<MainWindow>();
        }
    }
}
